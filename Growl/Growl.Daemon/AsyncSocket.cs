using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Net.Security;
using System.Text;

namespace Growl.Daemon
{
	/// <summary>
	/// The AsyncSocket class allows for asynchronous socket activity,
	/// and has usefull methods that allow for controlled reading of a certain length,
	/// or until a specified terminator.
	/// It also has the ability to timeout asynchronous reads, and has several delegate methods.
	/// </summary>
	public class AsyncSocket
	{
		public delegate void SocketWillDisconnect(AsyncSocket sender, Exception e);
		public delegate void SocketDidDisconnect(AsyncSocket sender);
		public delegate void SocketDidAccept(AsyncSocket sender, AsyncSocket newSocket);
		public delegate void SocketWillConnect(AsyncSocket sender, Socket socket);
		public delegate void SocketDidConnect(AsyncSocket sender, IPAddress address, UInt16 port);
		public delegate void SocketDidRead(AsyncSocket sender, Data data, long tag);
		public delegate void SocketDidReadPartial(AsyncSocket sender, int partialLength, long tag);
		public delegate void SocketDidWrite(AsyncSocket sender, long tag);
		public delegate void SocketDidWritePartial(AsyncSocket sender, int partialLength, long tag);
		public delegate void SocketDidSecure(AsyncSocket sender);
        // GROWL
        public delegate bool SocketDidReadTimeout(AsyncSocket sender);

		public event SocketWillDisconnect WillDisconnect;
		public event SocketDidDisconnect DidDisconnect;
		public event SocketDidAccept DidAccept;
		public event SocketWillConnect WillConnect;
		public event SocketDidConnect DidConnect;
		public event SocketDidRead DidRead;
		public event SocketDidReadPartial DidReadPartial;
		public event SocketDidWrite DidWrite;
		public event SocketDidWritePartial DidWritePartial;
		public event SocketDidSecure DidSecure;
        // GROWL
        public event SocketDidReadTimeout DidReadTimeout;
		
		private Socket socket4;
		private Socket socket6;
		private Stream stream;
		private NetworkStream socketStream;
		private SslStream secureSocketStream;

		private const int INIT_READQUEUE_CAPACITY = 5;
		private const int INIT_WRITEQUEUE_CAPACITY = 5;

		private const int CONNECTION_QUEUE_CAPACITY = 10;

		private const int READ_CHUNKSIZE    = (32 * 256);
		private const int READALL_CHUNKSIZE = (64 * 256);
		private const int WRITE_CHUNKSIZE   = (32 * 256);

		private volatile byte flags;
		private const byte kDidPassConnectMethod   =  1; // If set, disconnection results in delegate call.
		private const byte kDidCallConnectDelegate =  2; // If set, connect delegate has been called.
		private const byte kPauseReads             =  4; // If set, reads are not dequeued until further notice.
		private const byte kPauseWrites            =  8; // If set, writes are not dequeued until further notice.
		private const byte kForbidReadsWrites      = 16; // If set, no new reads or writes are allowed.
		private const byte kDisconnectAfterReads   = 32; // If set, disconnect as soon as no more reads are queued.
		private const byte kDisconnectAfterWrites  = 64; // If set, disconnect as soon as no more writes are queued.

		private Queue readQueue;
		private Queue writeQueue;

		private AsyncReadPacket currentRead;
		private AsyncWritePacket currentWrite;

		private System.Threading.Timer readTimer;
		private System.Threading.Timer writeTimer;

		private MutableData readOverflow;

		// We use a seperate lock object instead of locking on 'this'.
		// This is necessary to avoid a tricky deadlock situation.
		// The generated methods that handle += and -= calls to events actually lock on 'this'.
		// So the following is possible:
		// - We invoke one of our OnEventHandler methods from within a lock(this) block.
		// - There is a SynchronizedObject set, and we invoke callbacks on it.
		// - A registered delegate receives the callback on a seperate thread.
		// - The registered delegate then attempts to add a delegate to one of our events.
		// - Deadlock!
		// - The += method is blocking until we finish our lock(this) block.
		// - We won't finish our lock(this) block until the delegate methods complete. 
		private Object lockObj = new Object();

		/// <summary>
		/// The AsyncReadPacket encompasses the instructions for a read.
		/// The content of a read packet allows the code to determine if we're:
		/// reading to a certain length, reading to a certain separator, or simply reading the first chunk of data.
		/// </summary>
		private class AsyncReadPacket
		{
			public MutableData buffer;
			public int bytesDone;
			public int bytesProcessing;
			public int timeout;
			public long tag;
			public bool readAllAvailableData;
			public bool fixedLengthRead;
			public byte[] term;
			public IAsyncResult iar;

			public AsyncReadPacket(MutableData buffer,
			                               int timeout,
			                              long tag,
			                              bool readAllAvailableData,
			                              bool fixedLengthRead,
			                            byte[] term)
			{
				this.buffer = buffer;
				this.bytesDone = 0;
				this.bytesProcessing = 0;
				this.timeout = timeout;
				this.tag = tag;
				this.readAllAvailableData = readAllAvailableData;
				this.fixedLengthRead = fixedLengthRead;
				this.term = term;
				this.iar = null;
			}
		}

		/// <summary>
		/// The AsyncWritePacket encompasses the instructions for a write.
		/// </summary>
		private class AsyncWritePacket
		{
			public IData buffer;
			public int bytesDone;
			public int bytesProcessing;
			public int timeout;
			public long tag;
			public IAsyncResult iar;

			public AsyncWritePacket(IData buffer,
			                          int bytesDone,
			                          int timeout,
			                         long tag)
			{
				this.buffer = buffer;
				this.bytesDone = 0;
				this.bytesProcessing = 0;
				this.timeout = timeout;
				this.tag = tag;
			}
		}

		/// <summary>
		/// Encompasses special instructions for interruptions in the read/write queues.
		/// This class my be altered to support more than just TLS in the future.
		/// </summary>
		private class AsyncSpecialPacket
		{
			public bool startTLS;

			public AsyncSpecialPacket(bool startTLS)
			{
				this.startTLS = startTLS;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Setup
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public AsyncSocket()
		{
			// Initialize an empty set of flags
			// During execution, various flags are set to allow us to track what's been done
			// and what needs to be done.
			flags = 0;

			// Initialize read and write queues (thread safe)
			readQueue = Queue.Synchronized(new Queue(INIT_READQUEUE_CAPACITY));
			writeQueue = Queue.Synchronized(new Queue(INIT_WRITEQUEUE_CAPACITY));
		}

		private Object mTag;
		/// <summary>
		/// Gets or sets the object that contains data about the socket.
		/// <remarks>
		///		Any type derived from the Object class can be assigned to this property.
		///		A common use for the Tag property is to store data that is closely associated with the socket.
		/// </remarks>
		/// </summary>
		public Object Tag
		{
			get { return mTag; }
			set { mTag = value; }
		}

		protected virtual void OnSocketWillDisconnect(Exception e)
		{
			if(WillDisconnect != null)
			{
				Invoke(WillDisconnect, this, e);
			}
		}

		protected virtual void OnSocketDidDisconnect()
		{
			if (DidDisconnect != null)
			{
				Invoke(DidDisconnect, this);
			}
		}

		protected virtual void OnSocketDidAccept(AsyncSocket newSocket)
		{
			if (DidAccept != null)
			{
				Invoke(DidAccept, this, newSocket);
			}
		}

		protected virtual void OnSocketWillConnect(Socket socket)
		{
			if (WillConnect != null)
			{
				Invoke(WillConnect, this, socket);
			}
		}

		protected virtual void OnSocketDidConnect(IPAddress address, UInt16 port)
		{
			if (DidConnect != null)
			{
				Invoke(DidConnect, this, address, port);
			}
		}

		protected virtual void OnSocketDidRead(Data data, long tag)
		{
			if (DidRead != null)
			{
				Invoke(DidRead, this, data, tag);
			}
		}

		protected virtual void OnSocketDidReadPartial(int partialLength, long tag)
		{
			if (DidReadPartial != null)
			{
				Invoke(DidReadPartial, this, partialLength, tag);
			}
		}

		protected virtual void OnSocketDidWrite(long tag)
		{
			if (DidWrite != null)
			{
				Invoke(DidWrite, this, tag);
			}
		}

		protected virtual void OnSocketDidWritePartial(int partialLength, long tag)
		{
			if (DidWritePartial != null)
			{
				Invoke(DidWritePartial, this, partialLength, tag);
			}
		}

		protected virtual void OnSocketDidSecure()
		{
			if (DidSecure != null)
			{
				Invoke(DidSecure, this);
			}
		}

        // GROWL
        protected virtual bool OnSocketDidReadTimeout()
        {
            if (DidReadTimeout != null)
            {
                return (bool) Invoke(DidReadTimeout, this);
            }
            return false;
        }
		
		private System.ComponentModel.ISynchronizeInvoke mSynchronizingObject = null;
		/// <summary>
		/// Set the <see cref="System.ComponentModel.ISynchronizeInvoke">ISynchronizeInvoke</see>
		/// object to use as the invoke object. When returning results from asynchronous calls,
		/// the Invoke method on this object will be called to pass the results back
		/// in a thread safe manner.
		/// </summary>
		/// <remarks>
		/// If using in conjunction with a form, it is highly recommended
		/// that you pass your main <see cref="System.Windows.Forms.Form">form</see> (window) in.
		/// </remarks>
		public System.ComponentModel.ISynchronizeInvoke SynchronizingObject
		{
			get { return mSynchronizingObject; }
			set { mSynchronizingObject = value; }
		}

		private bool mAllowApplicationForms = true;
		/// <summary>
		/// Allows the application to attempt to post async replies over the
		/// application "main loop" by using the message queue of the first available
		/// open form (window). This is retrieved through
		/// <see cref="System.Windows.Forms.Application.OpenForms">Application.OpenForms</see>.
		/// 
		/// Note: This is true by default.
		/// </summary>
		public bool AllowApplicationForms
		{
			get { return mAllowApplicationForms; }
			set { mAllowApplicationForms = value; }
		}

		private bool mAllowMultithreadedCallbacks = false;
		/// <summary>
		/// If set to true, <see cref="AllowApplicationForms">AllowApplicationForms</see>
		/// is set to false and <see cref="SynchronizingObject">SynchronizingObject</see> is set
		/// to null. Any time an asynchronous method needs to invoke a delegate method
		/// it will run the method in its own thread.
		/// </summary>
		/// <remarks>
		/// If set to true, you will have to handle any synchronization needed.
		/// If your application uses Windows.Forms or any other non-thread safe
		/// library, then you will have to do your own invoking.
		/// </remarks>
		public bool AllowMultithreadedCallbacks
		{
			get { return mAllowMultithreadedCallbacks; }
			set
			{
				mAllowMultithreadedCallbacks = value;
				if (mAllowMultithreadedCallbacks)
				{
					mAllowApplicationForms = false;
					mSynchronizingObject = null;
				}
			}
		}

		/// <summary>
		/// Helper method to obtain a proper invokeable object.
		/// If an invokeable object is set, it's immediately returned.
		/// Otherwise, an open windows form is returned if available.
		/// </summary>
		/// <returns>An invokeable object, or null if none available.</returns>
		private System.ComponentModel.ISynchronizeInvoke GetInvokeObject()
		{
			if (mSynchronizingObject != null) return mSynchronizingObject;

			if (mAllowApplicationForms)
			{
				// Need to post it over control thread
				System.Windows.Forms.FormCollection forms = System.Windows.Forms.Application.OpenForms;

				if (forms != null && forms.Count > 0)
				{
					System.Windows.Forms.Control control = forms[0];
					return control;
				}
			}
			return null;
		}

		/// <summary>
		/// Calls a method using the objects invokable object (if provided).
		/// Otherwise, it simply invokes the method normally.
		/// </summary>
		/// <param name="method">
		///		The method to call.
		/// </param>
		/// <param name="args">
		///		The arguments to call the method with.
		/// </param>
		/// <returns>
		///		The result returned from method, or null if the method could not be invoked.
		///	</returns>
		protected object Invoke(Delegate method, params object[] args)
		{
			System.ComponentModel.ISynchronizeInvoke invokeable = GetInvokeObject();

			if (invokeable != null)
			{
				return invokeable.Invoke(method, args);
			}

			if (mAllowMultithreadedCallbacks)
			{
				return method.DynamicInvoke(args);
			}

			return null;
		}

		/// <summary>
		/// Allows invoke options to be inherited from another AsyncSocket.
		/// This is usefull when accepting connections.
		/// </summary>
		/// <param name="fromSocket">
		///		AsyncSocket object to copy invoke options from.
		///	</param>
		protected void InheritInvokeOptions(AsyncSocket fromSocket)
		{
			// We set the MultiThreadedCallback property first,
			// as it has the potential to affect the other properties.
			AllowMultithreadedCallbacks = fromSocket.AllowMultithreadedCallbacks;

			AllowApplicationForms = fromSocket.AllowApplicationForms;
			SynchronizingObject = fromSocket.SynchronizingObject;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Progress
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public float ProgressOfCurrentRead()
		{
			long tag;
			int bytesDone;
			int total;

			float result = ProgressOfCurrentRead(out tag, out bytesDone, out total);
			return result;
		}

		public float ProgressOfCurrentRead(out long tag, out int bytesDone, out int total)
		{
			// First get a reference to the current read.
			// We do this because the currentRead pointer could be changed in a separate thread.
			// And locking should not be done in this method
			// because it's public, and could potentially cause deadlock.
			AsyncReadPacket thisRead = null;
			Interlocked.Exchange(ref thisRead, currentRead);
			
			// Check to make sure we're actually reading something right now
			if (thisRead == null)
			{
				tag = ((long)0);
				bytesDone = 0;
				total = 0;
				return float.NaN;
			}
			
			// It's only possible to know the progress of our read if we're reading to a certain length
			// If we're reading to data, we of course have no idea when the data will arrive
			// If we're reading to timeout, then we have no idea when the next chunk of data will arrive.
			bool hasTotal = ((thisRead.readAllAvailableData == false) && (thisRead.term == null));
			
			tag = thisRead.tag;
			bytesDone = thisRead.bytesDone;
			total = hasTotal ? thisRead.buffer.Length : 0;
			
			if (total > 0)
				return (((float)bytesDone) / ((float)total));
			else
				return ((float)1.0);
		}

		public float ProgressOfCurrentWrite()
		{
			long tag;
			int bytesDone;
			int total;

			float result = ProgressOfCurrentWrite(out tag, out bytesDone, out total);
			return result;
		}

		public float ProgressOfCurrentWrite(out long tag, out int bytesDone, out int total)
		{
			// First get a reference to the current write.
			// We do this because the currentWrite pointer could be changed in a separate thread.
			// And locking should not be done in this method
			// because it's public, and could potentially cause deadlock.
			AsyncWritePacket thisWrite = null;
			Interlocked.Exchange(ref thisWrite, currentWrite);
			
			// Check to make sure we're actually writing something right now
			if (thisWrite == null)
			{
				tag = ((long)0);
				bytesDone = 0;
				total = 0;
				return float.NaN;
			}
			
			tag = thisWrite.tag;
			bytesDone = thisWrite.bytesDone;
			total = thisWrite.buffer.Length;
			
			return (((float)bytesDone) / ((float)total));
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Accepting
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Tells the socket to begin accepting connections on the given port.
		/// The socket will listen on all interfaces.
		/// Be sure to register to receive DidAccept events.
		/// </summary>
		/// <param name="port">
		///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
		/// </param>
		/// <returns>
		///		True if the socket was able to begin listening for connections on the given address and port.
		///		False otherwise.  If false consult the error parameter for more information.
		/// </returns>
		public bool Accept(UInt16 port)
		{
			Exception error;
			return Accept(null, port, out error);
		}

		/// <summary>
		/// Tells the socket to begin accepting connections on the given port.
		/// The socket will listen on all interfaces.
		/// Be sure to register to receive DidAccept events.
		/// </summary>
		/// <param name="port">
		///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
		/// </param>
		/// <param name="error">
		///		If this method returns false, the error will contain the reason for it's failure.
		/// </param>
		/// <returns>
		///		True if the socket was able to begin listening for connections on the given address and port.
		///		False otherwise.  If false consult the error parameter for more information.
		/// </returns>
		public bool Accept(UInt16 port, out Exception error)
		{
			return Accept(null, port, out error);
		}

		/// <summary>
		/// Tells the socket to begin accepting connections on the given address and port.
		/// Be sure to register to receive DidAccept events.
		/// </summary>
		/// <param name="hostaddr">
		///		A string that contains an IP address in dotted-quad notation for IPv4
		///		or in colon-hexadecimal notation for IPv6.
		///		For convenience, you may also pass the strings "loopback" or "localhost".
		/// </param>
		/// <param name="port">
		///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
		/// </param>
		/// <returns>
		///		True if the socket was able to begin listening for connections on the given address and port.
		///		False otherwise.  If false consult the error parameter for more information.
		/// </returns>
		public bool Accept(String hostaddr, UInt16 port)
		{
			Exception error;
			return Accept(hostaddr, port, out error);
		}

		/// <summary>
		/// Tells the socket to begin accepting connections on the given address and port.
		/// Be sure to register to receive DidAccept events.
		/// </summary>
		/// <param name="hostaddr">
		///		A string that contains an IP address in dotted-quad notation for IPv4
		///		or in colon-hexadecimal notation for IPv6.
		///		For convenience, you may also pass the strings "loopback" or "localhost".
		///		Pass null to listen on all interfaces.
		/// </param>
		/// <param name="port">
		///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
		/// </param>
		/// <param name="error">
		///		If this method returns false, the error will contain the reason for it's failure.
		/// </param>
		/// <returns>
		///		True if the socket was able to begin listening for connections on the given address and port.
		///		False otherwise.  If false consult the error parameter for more information.
		/// </returns>
		public bool Accept(String hostaddr, UInt16 port, out Exception error)
		{
			error = null;
			
			// Make sure we're not already listening for connections, or already connected
			if ((socket4 != null) || (socket6 != null))
			{
				String e = "Attempting to connect while connected or accepting connections. Disconnect first.";
				error = new Exception(e);
				return false;
			}

			// Extract proper IPAddress(es) from the given hostaddr
			IPAddress address4;
			IPAddress address6;
			if (hostaddr == null)
			{
				address4 = IPAddress.Any;
				address6 = IPAddress.IPv6Any;
			}
			else
			{
				if (hostaddr.Equals("loopback") || hostaddr.Equals("localhost"))
				{
					address4 = IPAddress.Loopback;
					address6 = IPAddress.IPv6Loopback;
				}
				else
				{
					try
					{
						IPAddress addr = IPAddress.Parse(hostaddr);
						if(addr.AddressFamily == AddressFamily.InterNetwork)
						{
							address4 = addr;
							address6 = null;
						}
						else if(addr.AddressFamily == AddressFamily.InterNetworkV6)
						{
							address4 = null;
							address6 = addr;
						}
						else
						{
							String format = "hostaddr ({0}) is not a valid IPv4 or IPv6 address";
							throw new Exception(String.Format(format, hostaddr));
						}
					}
					catch (Exception e)
					{
						error = e;
						return false;
					}
				}
			}
			
			// Watch out for versions of XP that don't support IPv6
			if (!Socket.OSSupportsIPv6)
			{
				if (address4 == null)
				{
					error = new Exception("Requesting IPv6, but OS does not support it.");
					return false;
				}
				address6 = null;
			}

			// Attention: Lock within public method.
			// Note: Should be fine since we can only get this far if the socket is null.
			lock (lockObj)
			{
				try
				{
					// Initialize socket(s)
					if(address4 != null)
					{
						// Initialize socket
						socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						
						// Always reuse address
						socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
						
						// Bind the socket to the proper address/port
						socket4.Bind(new IPEndPoint(address4, port));
						
						// Start listening (using the preset max pending connection queue size)
						socket4.Listen(CONNECTION_QUEUE_CAPACITY);
						
						// Start accepting connections
						socket4.BeginAccept(new AsyncCallback(socket_DidAccept), socket4);
					}
					if(address6 != null)
					{
						// Initialize socket
						socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
						
						// Always reuse address
						socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
						
						// Bind the socket to the proper address/port
						socket6.Bind(new IPEndPoint(address6, port));
						
						// Start listening (using the preset max pending connection queue size)
						socket6.Listen(CONNECTION_QUEUE_CAPACITY);
						
						// Start accepting connections
						socket6.BeginAccept(new AsyncCallback(socket_DidAccept), socket6);
					}
				}
				catch (Exception e)
				{
					error = e;
					if (socket4 != null)
					{
						socket4.Close();
						socket4 = null;
					}
					if (socket6 != null)
					{
						socket6.Close();
						socket6 = null;
					}
					return false;
				}

				flags |= kDidPassConnectMethod;
			}

			return true;
		}

		/// <summary>
		/// Description forthcoming
		/// </summary>
		/// <param name="iar"></param>
		private void socket_DidAccept(IAsyncResult iar)
		{
			// Any reason to lock here?
            bool keepListening = true;
            Socket socket = null;
            try
            {
                socket = (Socket)iar.AsyncState;

                Socket newSocket = socket.EndAccept(iar);
                AsyncSocket newAsyncSocket = new AsyncSocket();

                newAsyncSocket.InheritInvokeOptions(this);
                newAsyncSocket.PreConfigure(newSocket);

                OnSocketDidAccept(newAsyncSocket);

                newAsyncSocket.PostConfigure();
            }
            catch (ObjectDisposedException objDisposedEx)
            {
                // the listener has been shut down
                keepListening = false;
            }
            catch (Exception e)
            {
                //CloseWithException(e);
            }
            finally
            {
                // Keep listening for more connections
                if(keepListening && socket != null)
                    socket.BeginAccept(new AsyncCallback(socket_DidAccept), socket);
            }
		}

		/// <summary>
		/// Called to configure an AsyncSocket after an accept has occured.
		/// This is called before OnSocketDidAccept.
		/// </summary>
		/// <param name="socket">
		///		The newly accepted socket.
		/// </param>
		private void PreConfigure(Socket socket)
		{
			// Store socket
			if(socket.AddressFamily == AddressFamily.InterNetwork)
			{
				this.socket4 = socket;
			}
			else
			{
				this.socket6 = socket;
			}

			// Create NetworkStream from new socket
			socketStream = new NetworkStream(socket);
			stream = socketStream;
			flags |= kDidPassConnectMethod;
		}

		/// <summary>
		/// Called to configure an AsyncSocket after an accept has occured.
		/// This is called after OnSocketDidAccept.
		/// </summary>
		private void PostConfigure()
		{
			// Notify the delegate
			flags |= kDidCallConnectDelegate;
			OnSocketDidConnect(RemoteAddress, RemotePort);

			// Immediately deal with any already-queued requests.
			// Notice that we delay the call to allow execution in socket_DidAccept().
			ScheduleDequeueRead();
			ScheduleDequeueWrite();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Connecting
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Begins an asynchronous connection attempt to the specified host and port.
		/// Returns false if the connection attempt immediately failed, in which case the error parameter will be set.
		/// If this method succeeds, the delegate will be informed of the
		/// connection success/failure via the proper delegate methods.
		/// </summary>
		/// <param name="host">
		///		The host name or IP address to connect to.
		///		E.g. "deusty.com" or "70.85.193.226" or "2002:cd9:3ea8:0:88c8:b211:b605:ab59"
		/// </param>
		/// <param name="port">
		///		The port to connect to (eg. 80)
		/// </param>
		/// <returns>
		/// 	True if the socket was able to begin attempting to connect to the given host and port.
		///		False otherwise.
		/// </returns>
		public bool Connect(String host, UInt16 port)
		{
			Exception error;
			return Connect(host, port, out error);
		}

		/// <summary>
		/// Begins an asynchronous connection attempt to the specified host and port.
		/// Returns false if the connection attempt immediately failed, in which case the error parameter will be set.
		/// If this method succeeds, the delegate will be informed of the
		/// connection success/failure via the proper delegate methods.
		/// </summary>
		/// <param name="host">
		///		The host to connect to (eg. "deusty.com")
		/// </param>
		/// <param name="port">
		///		The port to connect to (eg. 80)
		/// </param>
		/// <param name="error">
		///		If this method returns false, the error will contain the reason for it's failure.
		/// </param>
		/// <returns>
		/// 	True if the socket was able to begin attempting to connect to the given host and port.
		///		False otherwise.  If false consult the error parameter for more information.
		/// </returns>
		public bool Connect(String host, UInt16 port, out Exception error)
		{
			error = null;
			
			// Make sure we're not already connected, or listening for connections
			if ((socket4 != null) || (socket6 != null))
			{
				String e = "Attempting to connect while connected or accepting connections. Disconnect first.";
				error = new Exception(e);
				return false;
			}

			// Attention: Lock within public method.
			// Note: Should be fine since we can only get this far if the socket is null.
			lock (this)
			{
				try
				{
					IPAddress[] addresses = Dns.GetHostAddresses(host);

					if (addresses.Length == 0)
					{
						throw new Exception(String.Format("Unable to resolve host \"{0}\"", host));
					}

					bool done = false;
					for(int i = 0; i < addresses.Length && !done; i++)
					{
						IPAddress address = addresses[i];

						if (address.AddressFamily == AddressFamily.InterNetwork)
						{
							// Initialize a new socket
							socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

							// Allow delegate to configure the socket if needed
							OnSocketWillConnect(socket4);

							// Attempt to connect with the given information
							socket4.BeginConnect(address, port, new AsyncCallback(socket_DidConnect), socket4);

							// Stop looping through addresses
							done = true;
						}
						else if (address.AddressFamily == AddressFamily.InterNetworkV6)
						{
							if (Socket.OSSupportsIPv6)
							{
								// Initialize a new socket
								socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

								// Allow delegate to configure the socket if needed
								OnSocketWillConnect(socket6);

								// Attempt to connect with the given information
								socket6.BeginConnect(address, port, new AsyncCallback(socket_DidConnect), socket4);

								// Stop looping through addresses
								done = true;
							}
						}
					}

					if (!done)
					{
						String format = "Unable to resolve host \"{0}\" to valid IPv4 or IPv6 address";
						throw new Exception(String.Format(format, host));
					}
				}
				catch (Exception e)
				{
					error = e;
					if (socket4 != null)
					{
						socket4.Close();
						socket4 = null;
					}
					if (socket6 != null)
					{
						socket6.Close();
						socket6 = null;
					}
					return false;
				}

				flags |= kDidPassConnectMethod;
			}

			return true;
		}

		/// <summary>
		/// Callback method when socket has connected (or was unable to connect).
		/// 
		/// This method is thread safe.
		/// </summary>
		/// <param name="iar">
		///		The state of the IAsyncResult refers to the socke that called BeginConnect().
		/// </param>
		private void socket_DidConnect(IAsyncResult iar)
		{
			// We lock in this method to ensure that the SocketDidConnect delegate fires before
			// processing any reads or writes. ScheduledDequeue methods may be lurking.
			// Also this ensures the flags are properly updated prior to any other locked method executing.
			lock (lockObj)
			{
				try
				{
					Socket socket = (Socket)iar.AsyncState;

					socket.EndConnect(iar);

					socketStream = new NetworkStream(socket);
					stream = socketStream;

					// Notify the delegate
					flags |= kDidCallConnectDelegate;
					OnSocketDidConnect(RemoteAddress, RemotePort);

					// Immediately deal with any already-queued requests.
					MaybeDequeueRead();
					MaybeDequeueWrite();
				}
				catch (Exception e)
				{
					CloseWithException(e);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Security
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Temporary variables for storing StartTLS information before we can actually start TLS
		private bool isTLSClient;
		private String tlsServerName;
		private RemoteCertificateValidationCallback tlsRemoteCallback;
		private LocalCertificateSelectionCallback tlsLocalCallback;

		/// <summary>
		/// Secures the stream using SSL/TLS.
		/// The socket is secured immediately following any pending reads/writes already in the queue.
		/// Any reads/writes scheduled after this call will travel over a secure connection.
		/// 
		/// Note: You can't just call this on any old connection.
		/// TLS requires support on both ends, and should be called in accordance with the protocol in use.
		/// 
		/// Note: You cannot pass a null serverName.
		/// If you don't know the server name, pass an empty string, and use the remote callback as needed.
		/// 
		/// If TLS fails to authenticate, the event WillCloseWithException will fire with the reason.
		/// </summary>
		/// <param name="serverName">
		///		The expected server name on the remote certificate.
		///		This cannot be null. If you don't know, pass an empty stream and use the remote callback as needed.
		/// </param>
		/// <param name="rcvc">
		///		A RemoteCertificateValidationCallback delegate responsible for
		///		validating the certificate supplied by the remote party.
		///		Pass null if you don't need this functionality.
		/// </param>
		/// <param name="lcsc">
		///		A LocalCertificateSelectionCallback delegate responsible for
		///		selecting the certificate used for authentication.
		///		Pass null if you don't need this functionality.
		/// </param>
		public void StartTLSAsClient(String serverName, RemoteCertificateValidationCallback rcvc,
		                                                  LocalCertificateSelectionCallback lcsc)
		{
			// Update tls variables - we'll need to refer to these later when we actually start tls
			isTLSClient = true;
			tlsServerName = serverName;
			tlsRemoteCallback = rcvc;
			tlsLocalCallback = lcsc;

			// Inject StartTLS packets into read and write queues.
			// Once all pending reads and writes have completed, the StartTLS procedure will commence.
			AsyncSpecialPacket startTlsPacket = new AsyncSpecialPacket(true);
			readQueue.Enqueue(startTlsPacket);
			writeQueue.Enqueue(startTlsPacket);

			// Schedule immediate calls to MaybeDequeueRead and MaybeDequeueWrite without blocking
			ScheduleDequeueRead();
			ScheduleDequeueWrite();
		}
		
		/// <summary>
		/// Secures the stream using SSL/TLS.
		/// The socket is secured immediately following any pending reads/writes already in the queue.
		/// Any reads/writes scheduled after this call will travel over a secure connection.
		/// 
		/// Note: You must use the LocalCertificateSelectionCallback to return the required certificate for a server.
		/// 
		/// If TLS fails to authenticate, the event WillCloseWithException will fire with the reason.
		/// </summary>
		/// <param name="rcvc">
		///		A RemoteCertificateValidationCallback delegate responsible for
		///		validating the certificate supplied by the remote party.
		///		Pass null if you don't need this functionality.
		/// </param>
		/// <param name="lcsc">
		///		A LocalCertificateSelectionCallback delegate responsible for
		///		selecting the certificate used for authentication.
		///		Pass null if you don't need this functionality.
		/// </param>
		public void StartTLSAsServer(RemoteCertificateValidationCallback rcvc, LocalCertificateSelectionCallback lcsc)
		{
			// Update tls variables - we'll need to refer to these later when we actually start tls
			isTLSClient = false;
			tlsRemoteCallback = rcvc;
			tlsLocalCallback = lcsc;

			// Inject StartTLS packets into read and write queues.
			// Once all pending reads and writes have completed, the StartTLS procedure will commence.
			AsyncSpecialPacket startTlsPacket = new AsyncSpecialPacket(true);
			readQueue.Enqueue(startTlsPacket);
			writeQueue.Enqueue(startTlsPacket);

			// Schedule immediate calls to MaybeDequeueRead and MaybeDequeueWrite without blocking
			ScheduleDequeueRead();
			ScheduleDequeueWrite();
		}

		/// <summary>
		/// Starts the TLS procedure ONLY if it's the correct time to do so.
		/// This is dependent on several variables, such as the kPause flags, connected property, etc.
		/// 
		/// This method is NOT thread safe, and should only be invoked via thread safe methods.
		/// </summary>
		private void MaybeStartTLS()
		{
			// We can't start TLS until all of the following are met:
			// - Any queued reads prior to the user calling StartTLS are complete
			// - Any queued writes prior to the user calling StartTLS are complete
			// - We're currently connected to a remote host
			// - We've setup our normal socketStream
			// - We haven't already started TLS
			// 
			// If not all these conditions are met then we're either not ready to start tls,
			// or we've already started and/or finished it.

			if ((flags & kPauseReads) > 0 &&
				(flags & kPauseWrites) > 0 &&
				(this.Connected) && 
				(socketStream != null) &&
				(secureSocketStream == null))
			{
				try
				{
					secureSocketStream = new SslStream(socketStream, true, tlsRemoteCallback, tlsLocalCallback);

					if (isTLSClient)
					{
						secureSocketStream.BeginAuthenticateAsClient(tlsServerName,
												   new AsyncCallback(secureSocketStream_DidFinish), null);
					}
					else
					{
						secureSocketStream.BeginAuthenticateAsServer(null,
												   new AsyncCallback(secureSocketStream_DidFinish), null);
					}
				}
				catch (Exception e)
				{
					// The most likely cause of this exception is a null tlsServerName.
					CloseWithException(e);
				}
			}
		}
		
		/// <summary>
		/// Called when the secureSocketStream has finished TLS initialization.
		/// If it failed, then the End methods will throw an exception detailing the problem.
		/// 
		/// This method is thread safe.
		/// </summary>
		/// <param name="iar"></param>
		private void secureSocketStream_DidFinish(IAsyncResult iar)
		{
			lock (lockObj)
			{
				try
				{
					if(isTLSClient)
					{
						secureSocketStream.EndAuthenticateAsClient(iar);
					}
					else
					{
						secureSocketStream.EndAuthenticateAsServer(iar);
					}
					
					// Update generic stream - everything goes through our encrypted stream now
					stream = secureSocketStream;
					
					// Update flags - unset pause flags
					flags ^= kPauseReads;
					flags ^= kPauseWrites;

					// Invoke delegate method if needed
					OnSocketDidSecure();
					
					// And finally, resume reading and writing
					MaybeDequeueRead();
					MaybeDequeueWrite();
				}
				catch (Exception e)
				{
					CloseWithException(e);
				}
			}
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Disconnecting
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Fires the WillDisconnect event, and then closes the socket.
		/// 
		/// This method is NOT thread safe, and should only be invoked via thread safe methods.
		/// </summary>
		/// <param name="e">
		/// 	The exception that occurred, to be sent to the client.
		/// </param>
		private void CloseWithException(Exception e)
		{
			if ((flags & kDidPassConnectMethod) > 0)
			{
				// Try to salvage what data we can
				RecoverUnreadData();

				// Let the delegate know, so it can try to recover if it likes.
				OnSocketWillDisconnect(e);
			}
			Close();
		}

		/// <summary>
		/// This method extracts any unprocessed data, and makes it available to the client.
		/// 
		/// Called solely from CloseWithException, which is only called from thread safe methods.
		/// </summary>
		private void RecoverUnreadData()
		{
			// Todo: Implement RecoverUnreadData
		}

		/// <summary>
		/// Clears the read and writes queues.
		/// Remember that the queues are synchronized/thread-safe.
		/// </summary>
		private void EmptyQueues()
		{
			if (currentRead != null) EndCurrentRead();
			if (currentWrite != null) EndCurrentWrite();

			readQueue.Clear();
			writeQueue.Clear();
		}

		/// <summary>
		/// Drops pending reads and writes, closes all sockets and stream, and notifies delegate if needed.
		/// </summary>
		private void Close()
		{
			EmptyQueues();

			if (secureSocketStream != null)
			{
				secureSocketStream.Close();
				secureSocketStream = null;
			}
			if (socketStream != null)
			{
				socketStream.Close();
				socketStream = null;
			}
			if (stream != null)
			{
				// Stream is just a pointer to the real stream we're using
				// I.e. it points to either socketStream of secureSocketStream
				// Thus we don't close it
				stream = null;
			}
			if (socket6 != null)
			{
				socket6.Close();
				socket6 = null;
			}
			if (socket4 != null)
			{
				socket4.Close();
				socket4 = null;
			}

			if ((flags & kDidCallConnectDelegate) > 0)
			{
				flags = 0;

				// Notify delegate that we're now disconnected.
				// Note that it's safe for the delegate to call Connect() from the callback method.
				OnSocketDidDisconnect();
			}
			else
			{
				flags = 0;
			}
		}

		/// <summary>
		/// Immediately stops all transfers, and releases any socket and stream resources.
		/// Any pending reads or writes are dropped.
		/// The AsyncSocket object may be reused after this method is called.
		/// 
		/// If the socket is already disconnected, this method does nothing.
		/// 
		/// Note: The SocketDidDisconnect method will be called.
		/// </summary>
		public void Disconnect()
		{
			// The reason we have a public Disconnect() method and a private Close() method is because
			// the functionality matches Socket.Disconnect().
			// Since AsyncSocket can be resused, it's not the same as Socket.Close().
			Close();
		}
		
		/// <summary>
		/// Immediately stops all transfers, and releases any stream resources.
		/// However, the base sockets are NOT closed, and the caller now has sole ownership of the sockets.
		/// 
		/// Note: The SocketDidDisconnect method will NOT be called.
		/// </summary>
		public void Disconnect(out Socket socket4, out Socket socket6)
		{
			socket4 = this.socket4;
			socket6 = this.socket6;
			
			this.socket4 = null;
			this.socket6 = null;
			
			flags = 0x00;
			
			Close();
		}
		
		/// <summary>
		/// Disconnects after all pending reads have completed.
		/// After calling this, the read and write methods will do nothing.
		/// The socket will disconnect even if there are still pending writes.
		/// </summary>
		public void DisconnectAfterReading()
		{
			flags |= kForbidReadsWrites;
			flags |= kDisconnectAfterReads;
			
			// Schedule an immediate call to MaybeDisconnect without blocking
			ScheduleDisconnect();
		}
		
		/// <summary>
		/// Disconnects after all pending writes have completed.
		/// After calling this, the read and write methods will do nothing.
		/// The socket will disconnect even if there are still pending reads.
		/// </summary>
		public void DisconnectAfterWriting()
		{
			flags |= kForbidReadsWrites;
			flags |= kDisconnectAfterWrites;
			
			// Schedule an immediate call to MaybeDisconnect without blocking
			ScheduleDisconnect();
		}
		
		/// <summary>
		/// Schedules a call to MaybeDisconnect without blocking.
		/// </summary>
		private void ScheduleDisconnect()
		{
			// Create a timer that will fire immediately on a seperate thread in the thread pool
			new System.Threading.Timer(new TimerCallback(UnscheduleDisconnect), null, 0, Timeout.Infinite);
		}
		
		/// <summary>
		/// Called as a result of a call to ScheduleDisconnect().
		/// This method is called on a background (worker) thread.
		/// </summary>
		/// <param name="state">Not used</param>
		private void UnscheduleDisconnect(Object state)
		{
			MaybeDisconnect();
		}
		
		private void MaybeDisconnect()
		{
			lock (lockObj)
			{
				if ((flags & kDisconnectAfterReads) > 0)
				{
					if ((readQueue.Count == 0) && (currentRead == null))
					{
						Disconnect();
					}
				}
				if ((flags & kDisconnectAfterWrites) > 0)
				{
					if ((writeQueue.Count == 0) && (currentWrite == null))
					{
						Disconnect();
					}
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Errors
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private Exception GetEndOfStreamException()
		{
			return new Exception("Socket reached end of stream.");
		}

		private Exception GetReadTimeoutException()
		{
			return new Exception("Read operation timed out.");
		}

		private Exception GetWriteTimeoutException()
		{
			return new Exception("Write operation timed out.");
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Diagnostics
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The Connected property gets the connection state of the Socket as of the last I/O operation.
		/// When it returns false, the Socket was either never connected, or is no longer connected.
		/// 
		/// Note that this functionallity matches normal Socket.Connected functionallity.
		/// </summary>
		public bool Connected
		{
			get
			{
				if(socket4 != null)
					return socket4.Connected;
				else
					return ((socket6 != null) && (socket6.Connected));
			}
		}

		/// <summary>
		/// Non-retarded method of Connected.
		/// Returns the logical answer to the question "Is this socket connected."
		/// </summary>
		public bool SmartConnected
		{
			get
			{
				if(socket4 != null)
					return GetIsSmartConnected(socket4);
				else
					return GetIsSmartConnected(socket6);
			}
		}

		private bool GetIsSmartConnected(Socket socket)
		{
            bool connected = false;
			if (socket != null && socket.Connected)
			{
                connected = socket.Connected;
				bool blockingState = socket.Blocking;
				try
				{
					byte[] tmp = new byte[1];

					//socket.Blocking = false;
                    ////socket.SendBufferSize = 0;
                    ////socket.NoDelay = true;
					//socket.Send(tmp, 0, 0);

                    //socket.Blocking = false;
                    //int n = socket.Send(tmp, 0, 0);
                    //if (n > 0) Console.WriteLine(String.Format("{0} - {1}", tmp[0], System.Text.Encoding.UTF8.GetString(tmp)));
                    //else Console.WriteLine("Socket @ {0} has disconnected", socket.RemoteEndPoint.ToString());
                    //if (n == 0) connected = false;

                    socket.Blocking = false;
                    int n = socket.Receive(tmp);
                    if(n > 0) Console.WriteLine(String.Format("{0} - {1}", tmp[0], System.Text.Encoding.UTF8.GetString(tmp)));
                    if (n == 0) connected = false;
				}
				catch (SocketException e)
				{
					// 10035 == WSAEWOULDBLOCK
					if (e.NativeErrorCode == 10035)
					{
						// Still Connected, but the Send would block
					}
					else
					{
						// Disconnected
                        connected = false;
					}
				}
				finally
				{
					socket.Blocking = blockingState;
				}

				//return socket.Connected;
                return connected;
			}
			
			return false;
		}

        /*
        private bool GetIsSmartConnected(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                bool blockingState = socket.Blocking;
                try
                {
                    byte[] tmp = new byte[1];

                    socket.Blocking = false;
                    socket.Send(tmp, 0, 0);
                }
                catch (SocketException e)
                {
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode == 10035)
                    {
                        // Still Connected, but the Send would block
                    }
                    else
                    {
                        // Disconnected
                    }
                }
                finally
                {
                    socket.Blocking = blockingState;
                }

                return socket.Connected;
            }

            return false;
        }
         * */

		public IPAddress RemoteAddress
		{
			get
			{
				if(socket4 != null)
					return GetRemoteAddress(socket4);
				else
					return GetRemoteAddress(socket6);
			}
		}

		public UInt16 RemotePort
		{
			get
			{
				if(socket4 != null)
					return GetRemotePort(socket4);
				else
					return GetRemotePort(socket6);
			}
		}

		public IPAddress LocalAddress
		{
			get
			{
				if(socket4 != null)
					return GetLocalAddress(socket4);
				else
					return GetLocalAddress(socket6);
			}
		}

		public UInt16 LocalPort
		{
			get
			{
				if (socket4 != null)
					return GetLocalPort(socket4);
				else
					return GetLocalPort(socket6);
			}
		}

		private IPAddress GetRemoteAddress(Socket socket)
		{
			if (socket != null && socket.Connected)
			{
                try
                {
                    IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;
                    if (ep != null)
                        return ep.Address;
                }
                catch
                {
                    // we might encounter a WSAEWOULDBLOCK exception, but we dont care
                }
			}
			return null;
		}

		private UInt16 GetRemotePort(Socket socket)
		{
			if (socket != null && socket.Connected)
			{
                try
                {
				    IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;
				    if (ep != null)
                        return (UInt16)ep.Port;
                }
                catch
                {
                    // we might encounter a WSAEWOULDBLOCK exception, but we dont care
                }
			}
			return 0;
		}

		private IPAddress GetLocalAddress(Socket socket)
		{
			if (socket != null)
			{
                try
                {
                    IPEndPoint ep = (IPEndPoint)socket.LocalEndPoint;
                    if (ep != null)
                        return ep.Address;
                }
                catch
                {
                    // we might encounter a WSAEWOULDBLOCK exception, but we dont care
                }
			}
			return null;
		}

		private UInt16 GetLocalPort(Socket socket)
		{
			if (socket != null)
			{
                try
                {
                    IPEndPoint ep = (IPEndPoint)socket.LocalEndPoint;
                    if (ep != null)
                        return (UInt16)ep.Port;
                }
                catch
                {
                    // we might encounter a WSAEWOULDBLOCK exception, but we dont care
                }
			}
			return 0;
		}

		public override string ToString()
		{
			// Todo: Add proper description for AsyncSocket
			return base.ToString();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Reading
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Reads the first available bytes on the socket.
		/// </summary>
		/// <param name="timeout">
		///		Timeout in milliseconds. Specify negative value for no timeout.
		/// </param>
		/// <param name="tag">
		///		Tag to identify read request.
		///	</param>
		public void Read(int timeout, long tag)
		{
			if ((flags & kForbidReadsWrites) > 0) return;

			MutableData buffer = new MutableData(0);

			// readQueue is synchronized
			readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, tag, true, false, null));

			// We schedule an immediate call to MaybeDequeueRead without blocking
			ScheduleDequeueRead();
		}

		/// <summary>
		/// Reads a certain number of bytes, and calls the delegate method when those bytes have been read.
		/// If length is 0, this method does nothing and no delgate methods are called.
		/// </summary>
		/// <param name="length">
		///		The number of bytes to read.
		/// </param>
		/// <param name="timeout">
		///		Timeout in milliseconds. Specify negative value for no timeout.
		/// </param>
		/// <param name="tag">
		///		Tag to identify read request.
		///	</param>
		public void Read(int length, int timeout, long tag)
		{
			if (length <= 0) return;
			if ((flags & kForbidReadsWrites) > 0) return;

			MutableData buffer = new MutableData(length);

			// readQueue is synchronized
			readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, tag, false, true, null));

			// We schedule an immediate call to maybeDequeueRead without blocking
			ScheduleDequeueRead();
		}

		/// <summary>
		/// Reads bytes up to and including the passed data paramter, which acts as a separator.
		/// The bytes and the separator are returned by the delegate method.
		/// 
		/// If you pass null or zero-length data as the separator, this method will do nothing.
		/// To read a line from the socket, use the line separator (eg. CRLF for HTTP) as the data parameter.
		/// Note that this method is not character-set aware, so if a separator can occur naturally
		/// as part of the encoding for a character, the read will prematurely end.
		/// </summary>
		/// <param name="term">
		///		The separator/delimeter to use.
		/// </param>
		/// <param name="timeout">
		///		Timeout in milliseconds. Specify negative value for no timeout.
		/// </param>
		/// <param name="tag">
		///		Tag to identify read request.
		/// </param>
		public void Read(byte[] term, int timeout, long tag)
		{
			if ((term == null) || (term.Length == 0)) return;
			if ((flags & kForbidReadsWrites) > 0) return;

			MutableData buffer = new MutableData(0);

			// readQueue is synchronized
			readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, tag, false, false, term));

			// We schedule an immediate call to maybeDequeueRead without blocking
			ScheduleDequeueRead();
		}

		/// <summary>
		/// Schedules a call to MaybeDequeueRead without blocking.
		/// </summary>
		private void ScheduleDequeueRead()
		{
			// Create a timer that will fire immediately on a seperate thread in the thread pool
			new System.Threading.Timer(new TimerCallback(UnscheduleDequeueRead), null, 0, Timeout.Infinite);
		}

		/// <summary>
		/// Called as a result of a call to ScheduleDequeueRead().
		/// This method is called on a background (worker) thread.
		/// </summary>
		/// <param name="state">Not used</param>
		public void UnscheduleDequeueRead(object state)
		{
			MaybeDequeueRead();
		}

		/// <summary>
		/// If possible, this method dequeues a read from the read queue and starts it.
		/// This is only possible if all of the following are true:
		///  1) any previous read has completed
		///  2) there's a read in the queue
		///  3) and the stream is ready.
		/// 
		/// This method is thread safe.
		/// </summary>
		private void MaybeDequeueRead()
		{
			lock (lockObj)
			{
				if ((currentRead == null) && (stream != null))
				{
					if((flags & kPauseReads) > 0)
					{
						// Don't do any reads yet.
						// We're waiting for TLS negotiation to start and/or finish.

						// Attempt to start TLS if needed.
						// This method won't do anything unless it's the proper time.
						// We call it here because a user may have called StartTLS() immediately after Connect().
						MaybeStartTLS();
					}
					else if(readQueue.Count > 0)
					{
						// Get the next object in the read queue
						Object nextRead = readQueue.Dequeue();

						if (nextRead is AsyncSpecialPacket)
						{
							// Next read packet is a special instruction packet.
							// Right now this can only mean a StartTLS instruction.
							AsyncSpecialPacket specialRead = (AsyncSpecialPacket)nextRead;

							// Update flags - this flag will be unset when TLS finishes
							flags |= kPauseReads;

							// And attempt to start TLS
							// This method won't do anything unless both kPauseReads and kPauseWrites are set.
							MaybeStartTLS();
						}
						else
						{
							// Get the new current read AsyncReadPacket
							currentRead = (AsyncReadPacket)nextRead;

							// Start time-out timer
							if (currentRead.timeout >= 0)
							{
								readTimer = new System.Threading.Timer(new TimerCallback(stream_DidNotRead),
																	   currentRead,
																	   currentRead.timeout,
																	   Timeout.Infinite);
							}

							// Do we have any overflow data that we've already read from the stream?
							if (readOverflow != null)
							{
								// Start reading from the overflow
								DoReadOverflow();
							}
							else
							{
								// Start reading from the stream
								DoStartRead();
							}
						}
					}
					else if((flags & kDisconnectAfterReads) > 0)
					{
						Disconnect();
					}
				}
			}
		}

		/// <summary>
		/// This method fills the currentRead buffer with data from the readOverflow variable.
		/// After this is properly completed, DoFinishRead is called to process the bytes.
		/// </summary>
		private void DoReadOverflow()
		{
			Debug.Assert(currentRead.bytesDone == 0);

			if (currentRead.readAllAvailableData)
			{
				// We're supposed to read what's available.
				// What we have in the readOverflow is what we have available, so just use it.

				currentRead.buffer = readOverflow;
				currentRead.bytesProcessing = readOverflow.Length;

				readOverflow = null;
			}
			else if (currentRead.fixedLengthRead)
			{
				if (currentRead.buffer.Length < readOverflow.Length)
				{
					byte[] src = readOverflow.ByteArray;
					byte[] dst = currentRead.buffer.ByteArray;

					Buffer.BlockCopy(src, 0, dst, 0, dst.Length);

					currentRead.bytesProcessing = dst.Length;

					readOverflow.TrimStart(dst.Length);

					// Note that this is the only case in which the readOverflow isn't emptied.
					// This is OK because the read is guaranteed to finish in DoFinishRead().
				}
				else
				{
					byte[] src = readOverflow.ByteArray;
					byte[] dst = currentRead.buffer.ByteArray;

					Buffer.BlockCopy(src, 0, dst, 0, src.Length);

					currentRead.bytesProcessing = src.Length;

					readOverflow = null;
				}
			}
			else
			{
				// We're reading up to a termination sequence
				// So we can just set the currentRead buffer to the readOverflow
				// and the DoStartRead method will automatically handle any further overflow.

				currentRead.buffer = readOverflow;
				currentRead.bytesProcessing = readOverflow.Length;

				readOverflow = null;
			}

			// At this point we've filled a currentRead buffer with some data
			// And the currentRead.bytesProcessing is set to the amount of data we filled it with
			// It's now time to process the data.
			DoFinishRead();
		}

		/// <summary>
		/// This method is called when either:
		///  A) a new read is taken from the read queue
		///  B) or when data has just been read from the stream, and we need to read more.
		/// 
		/// More specifically, it is called from either:
		///  A) MaybeDequeueRead()
		///  B) DoFinishRead()
		/// 
		/// The above methods are thread safe, or inherently thread safe, so this method is inherently thread safe.
		/// It is not explicitly thread safe though, and should not be called outside thread safe methods.
		/// </summary>
		private void DoStartRead()
		{
			try
			{
				// Perform an AsyncRead to notify us of when data becomes available on the socket.
				//
				// The following should be spelled out:
				// If the stream in use does not fork off a background thread for the callback,
				// then it's possible for the delegate to get called prior
				// to currentRead.iar being set below!
				// Thus we use a temporary local reference variable to
				// prevent overwriting a different packet's iar.

				AsyncReadPacket thisRead = currentRead;

				// Determine how much to read
				int size;
				if (thisRead.readAllAvailableData)
				{
					size = READALL_CHUNKSIZE;
				}
				else if (thisRead.fixedLengthRead)
				{
					// We're reading a fixed amount of data, into a fixed size buffer
					// We'll read up to the chunksize amount

					// The read method is supposed to return smaller chunks as they become available.
					// However, it doesn't seem to always follow this rule in practice.
					// 
					// size = thisRead.buffer.Length - thisRead.bytesDone;

					int left = thisRead.buffer.Length - thisRead.bytesDone;
					size = Math.Min(left, READ_CHUNKSIZE);
				}
				else
				{
					// We're reading up to a termination sequence
					size = READ_CHUNKSIZE;
				}

				// Ensure the buffer is big enough to fit all the data
				if (thisRead.buffer.Length < (thisRead.bytesDone + size))
				{
					thisRead.buffer.SetLength(thisRead.bytesDone + size);
				}

				thisRead.iar = stream.BeginRead(thisRead.buffer.ByteArray,         // buffer to read data into
												thisRead.bytesDone,                // buffer offset
												size,                              // max amout of data to read
												new AsyncCallback(stream_DidRead), // callback method
												thisRead);                         // callback info
			}
			catch (Exception e)
			{
				CloseWithException(e);
			}
		}
		
		/// <summary>
		/// Called after we've read data from the stream.
		/// We now call DoBytesAvailable, which will read and process further available data via the stream.
		/// 
		/// This method is thread safe.
		/// </summary>
		/// <param name="iar">AsyncState is AsyncReadPacket.</param>
		private void stream_DidRead(IAsyncResult iar)
		{
			lock (lockObj)
			{
				if (iar.AsyncState == currentRead)
				{
					try
					{
						currentRead.bytesProcessing = stream.EndRead(iar);

						if (currentRead.bytesProcessing > 0)
						{
							DoFinishRead();
						}
						else
						{
							CloseWithException(GetEndOfStreamException());
						}
					}
					catch (Exception e)
					{
						CloseWithException(e);
					}
				}
			}
		}

		/// <summary>
		/// Called after a read timeout timer fires.
		/// This will generally fire on an available thread from the thread pool.
		/// 
		/// This method is thread safe.
		/// </summary>
		/// <param name="state">state is AsyncReadPacket.</param>
		private void stream_DidNotRead(object state)
		{
				lock (lockObj)
			{
				if (state == currentRead)
				{
					EndCurrentRead();

                    // GROWL
                    bool cancelClose = this.OnSocketDidReadTimeout();

                    if(!cancelClose)
                        CloseWithException(GetReadTimeoutException());
				}
			}
		}

		/// <summary>
		/// This method is called when either:
		///  A) a new read is taken from the read queue
		///  B) or when data has just been read from the stream.
		/// 
		/// More specifically, it is called from either:
		///  A) DoReadOverflow()
		///  B) stream_DidRead()
		/// 
		/// The above methods are thread safe, so this method is inherently thread safe.
		/// It is not explicitly thread safe though, and should not be called outside thread safe methods.
		/// </summary>
		private void DoFinishRead()
		{
			Debug.Assert(currentRead != null);
			Debug.Assert(currentRead.bytesProcessing > 0);

			int totalBytesRead = 0;
			bool done = false;

			if(currentRead.readAllAvailableData)
			{
				// We're done because we read everything that was available (up to a max size).
				currentRead.bytesDone += currentRead.bytesProcessing;
				totalBytesRead = currentRead.bytesProcessing;
				currentRead.bytesProcessing = 0;

				done = true;
			}
			else if (currentRead.fixedLengthRead)
			{
				// We're reading up to a fixed size
				currentRead.bytesDone += currentRead.bytesProcessing;
				totalBytesRead = currentRead.bytesProcessing;
				currentRead.bytesProcessing = 0;

				done = currentRead.buffer.Length == currentRead.bytesDone;
			}
			else
			{
				// We're reading up to a terminator
				// So let's start searching for the termination sequence in the new data

				while (!done && (currentRead.bytesProcessing > 0))
				{
					currentRead.bytesDone++;
					totalBytesRead++;
					currentRead.bytesProcessing--;

					bool match = currentRead.bytesDone >= currentRead.term.Length;
					int offset = currentRead.bytesDone - currentRead.term.Length;

					for (int i = 0; match && i < currentRead.term.Length; i++)
					{
						match = (currentRead.term[i] == currentRead.buffer[offset + i]);
					}
					done = match;
				}
			}

			if (done)
			{
				// If there was any overflow data, extract it and save it
				// I.e. we received Y bytes, but only needed X bytes to finish the read (X < Y)
				if (currentRead.bytesProcessing > 0)
				{
					readOverflow = new MutableData(currentRead.buffer, currentRead.bytesDone, currentRead.bytesProcessing);
				}

				// Truncate any excess unused buffer space in the read packet
				currentRead.buffer.SetLength(currentRead.bytesDone);

				CompleteCurrentRead();
				ScheduleDequeueRead();
			}
			else
			{
				// We're not done yet, but we have read in some bytes
				OnSocketDidReadPartial(totalBytesRead, currentRead.tag);

				// It appears that we've read all immediately available data on the socket
				// So begin asynchronously reading data again
				DoStartRead();
			}
		}

		/// <summary>
		/// Completes the current read by ending it, and then informing the delegate that it's complete.
		/// 
		/// This method is called from DoFinishRead, which is inherently thread safe.
		/// Therefore this method is also inherently thread safe.
		/// It is not explicitly thread safe though, and should not be called outside thread safe methods.
		/// </summary>
		private void CompleteCurrentRead()
		{
			// Save reference to currentRead
			AsyncReadPacket completedRead = currentRead;

			// End the current read (this will nullify the currentRead variable)
			EndCurrentRead();

			// Notify delegate if possible
			OnSocketDidRead(completedRead.buffer, completedRead.tag);
		}

		/// <summary>
		/// Ends the current read by disposing and nullifying the read timer,
		/// and then nullifying the current read.
		/// </summary>
		private void EndCurrentRead()
		{
			if (readTimer != null)
			{
				readTimer.Dispose();
				readTimer = null;
			}

			currentRead = null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Writing
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Writes the specified data to the socket.
		/// </summary>
		/// <param name="data">
		///		An object that implements the IData interface.
		///		The object will be properly handled regardless of whether it's a stream or raw bytes.
		/// </param>
		/// <param name="timeout">
		///		Timeout in milliseconds. Specify a negative value if no timeout is desired.	
		/// </param>
		/// <param name="tag">
		///		A tag that can be used to track the write.
		///		This tag will be returned in the callback methods.
		/// </param>
		public void Write(IData data, int timeout, long tag)
		{
			if (data.Length == 0) return;

			writeQueue.Enqueue(new AsyncWritePacket(data, 0, timeout, tag));
			ScheduleDequeueWrite();
		}

		/// <summary>
		/// Schedules a call to MaybeDequeueWrite without blocking.
		/// </summary>
		private void ScheduleDequeueWrite()
		{
			// Create a timer that will fire immediately on a seperate thread in the thread pool
			new System.Threading.Timer(new TimerCallback(UnscheduleDequeueWrite), null, 0, Timeout.Infinite);
		}

		/// <summary>
		/// Called as a result of a call to ScheduleDequeueWrite().
		/// This method is called on a background (worker) thread.
		/// </summary>
		/// <param name="state">Not used</param>
		private void UnscheduleDequeueWrite(object state)
		{
			MaybeDequeueWrite();
		}

		/// <summary>
		/// If possible, this method dequeues a write from the write queue and starts it.
		/// This is only possible if all of the following are true:
		///  1) any previous write has completed
		///  2) there's a write in the queue
		///  3) and the socket is connected.
		/// 
		/// This method is thread safe.
		/// </summary>
		private void MaybeDequeueWrite()
		{
			lock (lockObj)
			{
				if ((currentWrite == null) && (stream != null))
				{
					if ((flags & kPauseWrites) > 0)
					{
						// Don't do any reads yet.
						// We're waiting for TLS negotiation to start and/or finish.

						// Attempt to start TLS if needed.
						// This method won't do anything unless it's the proper time.
						// We call it here because a user may have called StartTLS() immediately after Connect().
						MaybeStartTLS();
					}
					else if (writeQueue.Count > 0)
					{
						// Get the next object in the read queue
						Object nextWrite = writeQueue.Dequeue();

						if (nextWrite is AsyncSpecialPacket)
						{
							// Next write packet is a special instruction packet.
							// Right now this can only mean a StartTLS instruction.
							AsyncSpecialPacket specialWrite = (AsyncSpecialPacket)nextWrite;

							// Update flags - this flag will be unset when TLS finishes
							flags |= kPauseWrites;

							// And attempt to start TLS
							// This method won't do anything unless both kPauseReads and kPauseWrites are set.
							MaybeStartTLS();
						}
						else
						{
							// Get the current write AsyncWritePacket
							currentWrite = (AsyncWritePacket)nextWrite;

							// Start time-out timer
							if (currentWrite.timeout >= 0)
							{
								writeTimer = new System.Threading.Timer(new TimerCallback(stream_DidNotWrite),
																		currentWrite,
																		currentWrite.timeout,
																		Timeout.Infinite);
							}

							try
							{
								DoSendBytes();
							}
							catch (Exception e)
							{
								CloseWithException(e);
							}
						}
					}
					else if ((flags & kDisconnectAfterWrites) > 0)
					{
						Disconnect();
					}
				}
			}
		}

		/// <summary>
		/// Called when an asynchronous write has finished.
		/// This may just be a chunk of the data, and not the entire thing.
		/// 
		/// This method is thread safe.
		/// </summary>
		/// <param name="iar"></param>
		private void stream_DidWrite(IAsyncResult iar)
		{
			lock (lockObj)
			{
				if (iar.AsyncState == currentWrite)
				{
					try
					{
						// Note: EndWrite is void
						// Instead we must store and retrieve the amount of data we were trying to send
						stream.EndWrite(iar);
						currentWrite.bytesDone += currentWrite.bytesProcessing;

						if (currentWrite.bytesDone == currentWrite.buffer.Length)
						{
							CompleteCurrentWrite();
							ScheduleDequeueWrite();
						}
						else
						{
							// We're not done yet, but we have written out some bytes
							OnSocketDidWritePartial(currentWrite.bytesProcessing, currentWrite.tag);
							
							DoSendBytes();
						}
					}
					catch (Exception e)
					{
						CloseWithException(e);
					}
				}
			}
		}

		/// <summary>
		/// Called when a timeout occurs. (Called via thread timer).
		/// 
		/// This method is thread safe.
		/// </summary>
		/// <param name="state">
		/// 	The AsyncWritePacket that the timeout applies to.
		/// </param>
		private void stream_DidNotWrite(object state)
		{
			lock (lockObj)
			{
				if (state == currentWrite)
				{
					EndCurrentWrite();
					CloseWithException(GetWriteTimeoutException());
				}
			}
		}

		/// <summary>
		/// This method is called when either:
		///  A) a new write is taken from the write queue
		///  B) or when a previos write has finished.
		/// 
		/// More specifically, it is called from either:
		///  A) MaybeDequeueWrite()
		///  B) socket_DidSend()
		/// 
		/// The above methods are thread safe, so this method is inherently thread safe.
		/// It is not explicitly thread safe though, and should not be called outside the above named methods.
		/// </summary>
		private void DoSendBytes()
		{
			if (currentWrite.buffer.IsStream)
			{
				// We're dealing with an underlying stream
				// We'll read from the stream only as much data as we're prepared to send

				byte[] buffer;
				int size = currentWrite.buffer.ReadByteArray(out buffer, currentWrite.bytesDone, WRITE_CHUNKSIZE);

				// The following should be spelled out:
				// If the stream in use does not fork off a background thread for the callback,
				// then it's possible for the delegate to get called prior
				// to currentRead.iar being set below!
				// Thus we use a temporary local reference variable to
				// prevent overwriting a different packet's iar.

				AsyncWritePacket thisWrite = currentWrite;
				thisWrite.bytesProcessing = size;
				thisWrite.iar = stream.BeginWrite(buffer,                             // buffer to write from
												  0,                                  // buffer offset
												  size,                               // amount of data to send
												  new AsyncCallback(stream_DidWrite), // callback method
												  thisWrite);                         // callback info
				
			}
			else
			{
				// We're dealing with a memory store
				// No need to copy bytes around, we'll simply read straight from the buffer

				int available = currentWrite.buffer.Length - currentWrite.bytesDone;
				int size = (available < WRITE_CHUNKSIZE) ? available : WRITE_CHUNKSIZE;

				// The following should be spelled out:
				// If the stream in use does not fork off a background thread for the callback,
				// then it's possible for the delegate to get called prior
				// to currentRead.iar being set below!
				// Thus we use a temporary local reference variable to
				// prevent overwriting a different packet's iar.

				AsyncWritePacket thisWrite = currentWrite;
				thisWrite.bytesProcessing = size;
				thisWrite.iar = stream.BeginWrite(thisWrite.buffer.ByteArray,         // buffer to write from
												  thisWrite.bytesDone,                // buffer offset
												  size,                               // amount of data to send
												  new AsyncCallback(stream_DidWrite), // callback method
												  thisWrite);                         // callback info

			}
		}

		/// <summary>
		/// Completes the current write by ending it, and then informing the delegate that it's complete.
		/// 
		/// This method is called from stream_DidWrite, which is thread safe.
		/// Therefore this method is inherently thread safe.
		/// It is not explicitly thread safe though, and should not be called outside thread safe methods.
		/// </summary>
		private void CompleteCurrentWrite()
		{
			// Save reference to currentRead
			AsyncWritePacket completedWrite = currentWrite;

			// End the current write (this will nullify the currentWrite variable)
			EndCurrentWrite();

			// Notify delegate if possible
			OnSocketDidWrite(completedWrite.tag);
		}

		public void EndCurrentWrite()
		{
			if (writeTimer != null)
			{
				writeTimer.Dispose();
				writeTimer = null;
			}

			currentWrite = null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Static Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static byte[] CRLFCRLFData
        {
            get { return Encoding.UTF8.GetBytes("\r\n\r\n"); }
        }

		public static byte[] CRLFData
		{
			get { return Encoding.UTF8.GetBytes("\r\n"); }
		}

		public static byte[] CRData
		{
			get { return Encoding.UTF8.GetBytes("\r"); }
		}

		public static byte[] LFData
		{
			get { return Encoding.UTF8.GetBytes("\n"); }
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}
}
