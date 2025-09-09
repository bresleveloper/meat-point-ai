
call ngb.bat
cd "C:\Users\OWNER\Desktop\CODE\claude\MEAT-MEAL"
cd .\Meat-Point-AI\
xcopy "bin" "..\..\MEAT-MEAL-Deploy\bin" /s /e /y
cd .\AngularFront\dist\meat-point-ai\browser
xcopy "." "C:\Users\OWNER\Desktop\CODE\claude\MEAT-MEAL-Deploy\dist" /s /e /y
xcopy "index.html" "C:\Users\OWNER\Desktop\CODE\claude\MEAT-MEAL-Deploy" /y
cd "C:\Users\OWNER\Desktop\CODE\claude\MEAT-MEAL-Deploy"
tar.exe acvf dep.zip .
