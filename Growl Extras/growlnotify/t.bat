growlnotify "test"
echo %ERRORLEVEL%

growlnotify "test" /a:fake
echo %ERRORLEVEL%