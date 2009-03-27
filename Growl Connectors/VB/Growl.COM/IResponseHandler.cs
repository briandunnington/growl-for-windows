using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    /// <summary>
    /// This class is only used to help make COM interop possible.
    /// It should not be used from within any application code.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IResponseHandler
    {
        [DispId(1)]
        void OKReceived();

        [DispId(2)]
        void ErrorReceived(Error error);

        [DispId(3)]
        void CallbackReceived(CallbackData callbackData);
    }
}
