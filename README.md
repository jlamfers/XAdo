## XAdo ###

	// NOTE: normally you would create a singleton context
	//       on application initialization
	var context = new AdoContext( init => init
	    .SetConnectionStringName("AdventureWorks")
	    .KeepConnectionAlive(true) // ... more options available
	); 

	using (var session = context.CreateSession())
	{
       var dynamicList = 
	   session.Query("SELECT * FROM HumanResources.EmployeePayHistory WHERE PayFrequency IN @freqs",
                     new { freqs = new[] { 1, 2, 3 } }));

	   session.BeginTransaction();
	   session.Execute("DELETE HumanResources.EmployeePayHistory");
	   session.Rollback();

	   // you could continue here with additional SQL executions, with or without a transaction, 
	   // still using the same inner DbConnection
	}
    
XAdo extends ADO. It focuses on fast SQL execution. No SQL generation helpers are included. 

XAdo supports typed loading, i.e., SQL result can be mapped to typed entity results. Dynamic types
are supported as well (see example above). SQL results even can be mapped to interface types.

Handling multiple result sets and graph mapping (eager loading) are supported as well.

Custom type converters can be applied and can be implemented easily. Additional SQL server types like SqlGeography
and SqlGeometry are natively supported.

XAdo exposes an obvious API. It is applicable to any database that supports an ADO client implementation.

XAdo executes SQL by a lightweight context that lets you create sessions. The session supports transactions. 
Optionally the inner ADO connection can be kept open during the session's lifetime.

Async invocations are supported for all SQL execute methods.

XAdo is highly customizable by using a lightweight auto wiring dependency injector. 

All functional code is performant handwritten code. No code emitter is involved for SQL execution. 

A small customizable DTO emitter though is included for generating anonymous types (optionally, instead of dynamic types) 
and DTO implementations by interface. So you can bind a query result to an interface (or interfaces, in case of multi binding),
and you could extend the emitted DTO types by customizing the DTO emitter.


