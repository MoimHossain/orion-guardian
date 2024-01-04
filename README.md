# Orion-Guardian Extension

## Introduction

The **Orion-Guardian** is an Azure DevOps extension that provides some useful capabilities like Grouping of resources like repostiroy, environment etc and then apply permissions with custom role deinifions at scale.

> You can read further about the extension [here](https://moimhossain.com/2024/01/04/orion-guardian/)


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