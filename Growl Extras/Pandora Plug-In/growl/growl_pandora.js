GrowlPlugin = function(){
    var appName = "Pandora";
    var notificationName = "Track Changed";

    return {
        register : function(){
            var application = new Growl.Application();
            application.name = appName;
            // CHANGE THIS PATH IF YOU INSTALLED PANDORA IN ANOTHER LOCATION
            application.icon = "c:\\Program Files\\Pandora\\images\\icon\\icon-128.png";
            
            var notificationTypes = new Array();
            var nt1 = new Growl.NotificationType();
            nt1.name = notificationName;
            nt1.displayName = notificationName;
            nt1.enabled = true;
            notificationTypes.push(nt1);
            
            Growl.register(application, notificationTypes);
        },

        notify : function(track){
            // THIS WOULD BE A GOOD SPOT TO GO FETCH ALBUM ART (LAST.FM, AMAZON, ETC)

            var notification = new Growl.Notification();
            notification.name = notificationName;
            //notification.title = track.title;
            //notification.text = track.artist.name;
            notification.title = "Pandora tracked changed.";
            notification.text = "Now playing '" + track.title + "' by '" + track.artist.name + "'.";
            
            Growl.notify(appName, notification);
        }
    }
}();

TunerComms.onReady(function(){
    var config = {};

    // SET THIS VALUE THIS IF YOUR GROWL INSTANCE REQUIRES A PASSWORD
    //config.password = "secret";

    config.swfPath = "../growl/";
    config.encryptionAlgorithm = Growl.EncryptionAlgorithm.PlainText;
    config.onready2 = GrowlPlugin.register;
    Growl.init(config);
});

TunerComms.onTrack(function(track){
    if(track){
        GrowlPlugin.notify(track);
    }
});
