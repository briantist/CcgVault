# CcgVault
Windows Containers CCG Plugin for HashiCorp Vault

## What is this for?

In short, it's for running Windows containers with a gMSA (Group Managed Service Account) domain identity, without joining the container host to a domain.

See also:
- [gMSA architecture and improvements](https://docs.microsoft.com/en-us/virtualization/windowscontainers/manage-containers/manage-serviceaccounts#gmsa-architecture-and-improvements)

### How it works

The CCG (Container Credential Guard) handles retrieval of the gMSA's managed password, and use of those credentials to get and maintain a Kerberos ticket that is assigned to the running container. That allows anything running as `SYSTEM` or `Network Service` in the container to authenticate on the network as the gMSA, giving it access to domain resources.

With a domain joined container host, the computer's credentials are used to retrieve the gMSA's managed password. This is typically how gMSAs are used.

With a container host that is not domain joined, the credentials of a standard user account in the domain that has permission to retrieve the gMSA's managed password, must be provided to the CCG, so that it can use those credentials in place of the machine account.

The only way to provide these credentials to the CCG is via an out-of-process COM plugin that implements the [`ICcgDomainAuthCredentials` interface](https://docs.microsoft.com/en-us/windows/win32/api/ccgplugins/nn-ccgplugins-iccgdomainauthcredentials).

### So, what is this project?

This project is a plugin for providing the necessary credentials to the CCG, with HashiCorp Vault as the source of the credentials.

# Coming soon

What is available here is a sort of MVP of this, with the capability to retrieve credentials from kv1 and kv2 secret stores, and to authenticate via token.

An MSI package is also being built for easy installation, and I am in the process of writing tests to ensure that both the plugin and the installer continue functioning through changes and enhancements.

I expect a first release within the next few weeks, along with some basic documentation.
