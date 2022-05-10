docker build -t dtms:1.0 .

docker run -d --name dtms dtms:1.0

docker cp dtms:C:\src\SecureDocuments.WPF\bin\Release\net6.0-windows\win10-x64\publish\SecureDocuments.wpf.exe .\SecureDocuments_win10_64.exe
docker cp dtms:C:\src\SecureDocuments.WPF\bin\Release\net6.0-windows\win7-x64\publish\SecureDocuments.wpf.exe .\SecureDocuments_win7_64.exe
docker cp dtms:C:\src\SecureDocuments.WPF\bin\Release\net6.0-windows\win7-x86\publish\SecureDocuments.wpf.exe .\SecureDocuments_win7_86.exe

docker stop dtms
docker rm dtms


# docker run -it --rm --name dtms -v C:\dev\projects\github\OffersDocumentManagementApp:C:\project\dtms\ dtms:1.0 pwsh