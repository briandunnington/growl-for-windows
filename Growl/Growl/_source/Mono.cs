#if GTK
using Gtk;
using Gdk;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Growl.Mono
{

	public class NotifyIcon 
	{
		private StatusIcon status_icon;
		private Gtk.Menu _menu;
		private Dictionary<ImageMenuItem, ToolStripItem> menuItems;
		private Form mainForm;
		public NotifyIcon()
		{
			Gdk.Threads.Enter();
			status_icon = new StatusIcon(); 
			status_icon.Visible = false;
			Gdk.Threads.Leave();
			menuItems = new Dictionary<ImageMenuItem, ToolStripItem>();

		}
		
		public Form MainForm
		{
			get{ return this.mainForm;}
			set { this.mainForm = value; }
		}
		
		public event EventHandler DoubleClick
		{
			add
			{
				Gdk.Threads.Enter();
				status_icon.Activate += value;
				Gdk.Threads.Leave();
			}
			
			remove
			{
				Gdk.Threads.Enter();
				status_icon.Activate -= value;
				Gdk.Threads.Leave();
			}
		}
		
		
		public void Dispose()
		{
			Gdk.Threads.Enter();
			status_icon.Visible = false;
			Gdk.Threads.Leave();
			status_icon = null;
		}
		
		private void SetMenu(System.Windows.Forms.ContextMenuStrip menustrip)
		{
			Gdk.Threads.Enter();
			_menu = new Gtk.Menu();
			menuItems.Clear();
			foreach(ToolStripItem item in menustrip.Items)
			{
				ImageMenuItem menu_item = new ImageMenuItem(item.Text);
				menu_item.Visible = true;
				item.Tag = menu_item;
				menuItems.Add(menu_item, item);
				menu_item.Activated += new EventHandler(menu_item_Activated);
				_menu.Add(menu_item);
			}
			status_icon.PopupMenu += new PopupMenuHandler(status_icon_PopupMenu);
			Gdk.Threads.Leave();
		}
		
		
		private void status_icon_PopupMenu(object sender, PopupMenuArgs e)
		{
			foreach(Widget item in _menu.Children)
			{
				if(item.Visible) item.ShowAll();
			}
			_menu.Popup();
		}
		
		private void menu_item_Activated(object sender, EventArgs e)
		{
			ImageMenuItem item = sender as ImageMenuItem;
			if(item == null) return;
			ToolStripItem orig_item = menuItems[item] as ToolStripItem;
			if(orig_item == null) return;
			if(this.mainForm != null) this.mainForm.BeginInvoke(new MethodInvoker(orig_item.PerformClick));
		}
		
		private void SetIcon(System.Drawing.Icon icon)
		{
			Gdk.Threads.Enter();
			System.IO.MemoryStream _mem = new System.IO.MemoryStream();
			System.Drawing.Bitmap bmp = icon.ToBitmap();
			bmp.Save(_mem, System.Drawing.Imaging.ImageFormat.Png);
			_mem.Seek(0, System.IO.SeekOrigin.Begin);
			status_icon.Pixbuf = new Pixbuf(_mem);
			Gdk.Threads.Leave();
		}
		
		
		
		public string Text
		{
			set
			{
				status_icon.Tooltip = value;
			}
		}
		
		public bool Visible
		{
			set
			{
				Gdk.Threads.Enter();
				status_icon.Visible = value;
				Gdk.Threads.Leave();
			}
		}
		
		public System.Windows.Forms.ContextMenuStrip ContextMenuStrip
		{
			set
			{
				SetMenu(value);
			}
		}
		
		public System.Drawing.Icon Icon
		{
			set
			{
				SetIcon(value);
			}
		}
		
	}

}
#endif