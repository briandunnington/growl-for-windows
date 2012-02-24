Growl = function () {
	var swfID = "growlconnector";
	var swfUrl = "GrowlFlashConnector.swf";
	var swf;
	var scope = "Growl";
	var containerID = null;
	var defaultContainerID = "growlconnectorcontainer";
	var password = null;
	var passwordHashAlgorithm;
	var encryptionAlgorithm;

	function generateSWF(containerID, scope, swfUrl) {
		document.getElementById(containerID).innerHTML += "<!--[if IE]> \
                                                          <object classid='clsid:D27CDB6E-AE6D-11cf-96B8-444553540000' width='1' height='1' id='growlconnector' name='growlconnector'> \
                                                             <param name='movie' value='" + swfUrl + "'> \
                                                             <param name='allowscriptaccess' value='always'> \
                                                             <param name='flashvars' value='scope=" + scope + "'> \
                                                          </object> \
                                                          <![endif]--> \
                                                          <![if !IE]> \
                                                          <embed id='growlconnector' \
                                                             name='growlconnector' \
                                                             src='" + swfUrl + "' \
                                                             width='1' \
                                                             height='1' \
                                                             allowscriptaccess='always' \
                                                             type='application/x-shockwave-flash' \
                                                             flashvars='scope=" + scope + "' \
                                                             /> \
                                                          <![endif]>";
	}

	function oncallbackInternal(notificationID, action, context, type, timestamp) { }
	function onerrorInternal(response) { }

	return {
		oncallback: oncallbackInternal,
		onerror: onerrorInternal,

		onok: function () {
			//alert("ok");
		},

		onready: function () {
			swf = document.getElementById(swfID);
			swf.setPassword(password);
			swf.setPasswordHashAlgorithm(passwordHashAlgorithm);
			swf.setEncryptionAlgorithm(encryptionAlgorithm);
			Growl.onready2();
		},

		onready2: function () { },

		init: function (config) {
			passwordHashAlgorithm = Growl.PasswordHashAlgorithm.MD5;
			encryptionAlgorithm = Growl.EncryptionAlgorithm.PlainText;
			Growl.onerror = onerrorInternal;

			if (config && config.scope) scope = config.scope;
			if (config && config.containerID) containerID = config.containerID;
			if (config && config.swfUrl) swfUrl = config.swfUrl;
			if (config && config.password) password = config.password;
			if (config && config.passwordHashAlgorithm) passwordHashAlgorithm = config.passwordHashAlgorithm;
			if (config && config.encryptionAlgorithm) encryptionAlgorithm = config.encryptionAlgorithm;
			if (config && config.oncallback) Growl.oncallback = config.oncallback;
			if (config && config.onerror) Growl.onerror = config.onerror;
			if (config && config.onready2) Growl.onready2 = config.onready2;

			if (!containerID) {
				var container = document.createElement("div");
				container.id = defaultContainerID;
				document.body.appendChild(container);
				containerID = container.id;
			}
			generateSWF(containerID, scope, swfUrl);
		},

		register: function (application, notificationTypes) {
			swf.register(application, notificationTypes);
		},

		notify: function (appName, notification) {
			if (!notification.id) notification.id = new Date().getTime();
			swf.notify(appName, notification);
		},

		debug: function (msg) {
			prompt("DEBUG:", msg);
		}
	}
} ();

Growl.Application = function () {
	this.name = null;
	this.icon = null;
	this.customAttributes = {};
}

Growl.NotificationType = function () {
	this.name = null;
	this.displayName = null;
	this.icon = null;
	this.enabled = false;
}

Growl.Notification = function () {
	this.id = null;
	this.name = null;
	this.title = null;
	this.text = null;
	this.icon = null;
	this.priority = Growl.Priority.Normal;
	this.sticky = false;
	this.callback = {};
	this.callback.context = null;
	this.callback.type = null;
	this.callback.target = null;
	this.callback.method = Growl.CallbackTargetMethod.Get;
}

Growl.PasswordHashAlgorithm = {};
Growl.PasswordHashAlgorithm.MD5 = "MD5";
Growl.PasswordHashAlgorithm.SHA1 = "SHA1";
Growl.PasswordHashAlgorithm.SHA256 = "SHA256";

Growl.EncryptionAlgorithm = {};
Growl.EncryptionAlgorithm.PlainText = "NONE";
Growl.EncryptionAlgorithm.AES = "AES";
Growl.EncryptionAlgorithm.DES = "DES";
Growl.EncryptionAlgorithm.TripleDES = "3DES";

Growl.Priority = {};
Growl.Priority.Emergency = 2;
Growl.Priority.High = 1;
Growl.Priority.Normal = 0;
Growl.Priority.Moderate = -1;
Growl.Priority.VeryLow = -2;

Growl.CallbackAction = {};
Growl.CallbackAction.Click = "CLICK";
Growl.CallbackAction.Close = "CLOSE";
Growl.CallbackAction.TimedOut = "TIMEDOUT";

Growl.CallbackTargetMethod = {};
Growl.CallbackTargetMethod.Get = "GET";
Growl.CallbackTargetMethod.Post = "POST";