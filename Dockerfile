FROM microsoft/dotnet:2.1-aspnetcore-runtime as publish
EXPOSE $PORT
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "TresetaApp.dll"]
