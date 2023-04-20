# Project Title

Fellowmind.OData.Client

## Description

Library for wrapping generated OData client class that contains generic methods for accessing entities.

## Getting Started

### Dependencies

* Microsoft.OData.Client
* Microsoft.OData.Extensions.Client

### Usage

* Create a new class that inherits from Fellowmind.OData.Client.ODataClient
```
public class CrmODataClient : ODataClient<Microsoft.Dynamics.CRM.System>, ICrmODataClient
{
	public CrmODataClient(Microsoft.Dynamics.CRM.System system) : base(system)
	{
		
	}
}

public interface ICrmODataClient : IODataClient<Microsoft.Dynamics.CRM.System>
{
}
```

* Query example
```
await client.GetEntityAsync<Account>(Guid.NewGuid());
```

## Authors

Contributors names and contact info

* Juho Vainio, Mikko Kunnari
