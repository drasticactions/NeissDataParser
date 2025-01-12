ROOT=$(PWD)
APP_ROOT=$(ROOT)/src/NeissDataParser
APP_PROJECT=$(APP_ROOT)/NeissDataParser.csproj
BUILD_TYPE=Release
ARTIFACTS_DIR=$(ROOT)/artifacts

app_linux:
	rm -rf $(ARTIFACTS_DIR)/linux-x64
	dotnet build $(APP_PROJECT) -c $(BUILD_TYPE) -r linux-x64
	dotnet publish $(APP_PROJECT) -c $(BUILD_TYPE) -r linux-x64 -o $(ARTIFACTS_DIR)/linux-x64