# Orion-Guardian Extension

![Preview](OrionGuardian.gif)

## Introduction

The **Orion-Guardian** is an Azure DevOps extension that provides some useful capabilities like Grouping of resources like repostiroy, environment etc and then apply permissions with custom role deinifions at scale.

## Documentation

- [What is in it?](https://moimhossain.com/2024/01/04/orion-guardian/)
- [How to install](https://moimhossain.com/2024/01/04/orion-guardian/)
  - [Installing Extension](https://moimhossain.com/2024/01/04/orion-guardian/)
  - [Installing Backend](https://moimhossain.com/2024/01/04/orion-guardian/)

# Front end

## Prepare DevContainer

- Installing TFX CLI
```
sudo npm install -g tfx-cli
```
- Remove OPEN SSL error
```
export NODE_OPTIONS=--openssl-legacy-provider
```

On Windows,
```
$env:NODE_OPTIONS = "--openssl-legacy-provider"
```


# Building Backend

You can rebuild the backends and use your own container if you wish. 

## Building the API

Run this from the directory ([src/backend](src/backend)) where the solution file is located:

```bash
docker build -t moimhossain/azdo-control-panel:v2 -f "./Stellaris.WebApi/Dockerfile" .

docker push moimhossain/azdo-control-panel:v2
```

## Building the Daemon

Run this from the directory ([src/backend](src/backend)) where the solution file is located:

```bash
docker build -t moimhossain/azdo-control-panel-daemon:v2 -f "./Stellaris.Console/Dockerfile" .

docker push moimhossain/azdo-control-panel-daemon:v2
```
 
## Running locally

You can run the API container locally by running the following command:

```bash
docker run --rm \
    --env AZURE_DEVOPS_USE_PAT="true" \
    --env AZURE_DEVOPS_PAT="YOUR PAT TOKEN" \ 
    --env AZURE_COSMOS_CONNECTIONSTRING="AccountEndpoint=https://***.documents.azure.com:443/;AccountKey=***;" \
    --env AZURE_COSMOS_DATABASEID="stellaris" \
    -p 8080:8080  moimhossain/azdo-control-panel:v2
```
## Running the Daemon locally

You can run the Daemon container locally by running the following command:

```bash
docker run --rm \
    --env AZURE_DEVOPS_USE_PAT="true" \
    --env AZURE_DEVOPS_PAT="YOUR PAT TOKEN" \ 
    --env AZURE_COSMOS_CONNECTIONSTRING="AccountEndpoint=https://***.documents.azure.com:443/;AccountKey=***;" \
    --env AZURE_COSMOS_DATABASEID="stellaris" \
    moimhossain/azdo-control-panel-daemon:v2
```

That is it. You can now access the API at http://localhost:8080
