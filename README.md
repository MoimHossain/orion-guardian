# azdo-neptune-extension

## Introduction

The **azdo-neptune-extension** is an Azure DevOps extension that provides some useful capabilities like Repo group, AI copilot, migration etc. This project aims to assist developers in their daily development tasks by automating some of the repetitive and time-consuming operations.

## Key features

- **Repo group:** The extension allows users to group and view multiple Git repositories in a single screen. This feature can be very useful for teams working on multiple repositories simultaneously.
- **AI Copilot:** The AI Copilot feature uses machine learning algorithms to suggest code snippets based on the context of the current code. This can help developers write code faster and with fewer errors.
- **Migration:** The Migration feature allows users to export/import data from one Azure DevOps instance to another. This can be useful when moving between different organizations or when changing Azure DevOps plan types.

## Installation

To install the **azdo-neptune-extension**, follow these steps:

1. Go to the Azure DevOps Marketplace and search for the extension.
2. Click on the **Install** button and follow the instructions.
3. Once the extension is installed, it will be available for use in your Azure DevOps organization.

## Usage

Once the extension is installed, you can start using it. Here are some of the key usage scenarios:

- **Repo group:** To create a repo group, go to the Azure DevOps project and select the **Repo Group** option from the menu. You can then select the repositories you want to group together and give the group a name.
- **AI Copilot:** The AI Copilot feature is available in the code editor. As you start typing your code, the AI Copilot will suggest code snippets based on the context of your code.
- **Migration:** To export/import data using the Migration feature, go to the Azure DevOps organization and select the **Export/Import** option from the menu. You can then select the data you want to export/import and follow the instructions on the screen.

## Conclusion

Overall, the **azdo-neptune-extension** is a powerful tool that can help developers be more productive and efficient. With features like Repo group, AI Copilot, and Migration, you can automate many of the repetitive and time-consuming tasks involved in software development.

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