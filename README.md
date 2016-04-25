## XAdo ###

	var context = new AdoContext( initializer => initializer
	    .SetConnectionStringName("AdventureWorks")
	    .KeepConnectionAlive(true) // ... more options available
	); // normally create a singleton context

	using (var session = context.CreateSession())
	{
       var dynamicList = session.Query("session.Query("SELECT * FROM HumanResources.EmployeePayHistory WHERE PayFrequency IN @freqs",
                         new { freqs = new[] { 1, 2, 3 } }));

	   session.BeginTransaction();
	   session.Execute("DELETE HumanResources.EmployeePayHistory");
	   session.Rollback();

	   // you could continue here with additional SQL executions, with or without a transaction, still using the same inner ADO connection
	}
    
XAdo extends ADO. It focuses on SQL execution. No SQL generation helpers are included. 

XAdo uses an obvious API. It is applicable to any database that supports an ADO client implementation.

XAdo executes SQL by a lightweight context that lets you create sessions. The session supports transactions. 
Optionally the inner ADO connection can be kept open during the session's lifetime.

XAdo supports eager loading, i.e., loading a graph in one call.

Custom type converters can be applied or implemented easily.

Async invocations are supported for all SQL execute methods.

XAdo is highly customizable by using a lightweight auto wiring dependency injector. All functional code is 
performant handwritten code. No code emitter is involved for SQL execution. 

A small customizable DTO emitter though is included for generating anonymous types (optionally, instead of dynamic types) 
and DTO implementations by interface. So you can bind a query result to an interface (or interfaces, in case of multi binding),
and you could extend the emitted DTO types by customizing the DTO emitter.

A Dapper alike DbConnection extension class is included.

