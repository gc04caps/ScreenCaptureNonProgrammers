@Echo off
TITLE Stream Capture for Non-Programmers
CD .//bin/
ECHO Welcome to Screen Capture for Non-Programmers Ad-Hoc. This will set a single recording. 
ECHO Did you read the instructions and enter your username, password, etc, and create the folders?
ECHO Did you kill any running instance of the "infinite loop" StreamCapture?
echo.
Set /P _dur=How long (in minutes) do you want to record? 
Set /P _chan=What channel number do you want to record? 
Set /P _name=What is the file name - do not add .mp4! 
echo.
Echo You will rercord Channel [%_chan%] for [%_dur%] minutes and save to a file called [%_name%.mp4] 
Echo Is this correct?
echo.
PAUSE
echo.
CMD /K  StreamCapture.exe --duration %_dur% --channels %_chan% --filename %_name%
