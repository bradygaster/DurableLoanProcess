# Durable Loans Processing

This demo shows how .NET Core can be used to build cloud-native applications than run as microservices, or on serverles platforms, and that support a wide variety of communication patterns including SignalR for real-time communication and gRPC. 

## Project Structure

The solution consists of these projects, each of which serve an individual purpose. 


|Project |Purpose |
|---|---|
|DurableLoans.DomainModel   |A series of classes representing the ontology of the overall application.  |
|DurableLoans.ExchangeRateService   |gRPC service that provides currency exchange rate conversion.|
|DurableLoans.LoanOfficerNotificationService   |Back-end service containing both a REST API and a gRPC endpoint. The REST API receives requests from the Durable Function. When it receives requests it marshalls those over to a gRPC endpoint, which then streams that data out to a client used by the loan officer to provide final approval of loan applications.|
|DurableLoans.LoanProcess   |The Azure Function that serves as the back-end for the system.|
|DurableLoans.Web   |The front-end web app.|

## Get it running

1. Configure the `DurableLoans.LoanProcess` project with the correct Azure SignalR Service and Azure Storage connection strings. 
1. `func start` the `DurableProcess.LoanProcess`.
1. `dotnet run` the `DurableLoans.ExchangeRateService` project.
1. `dotnet run` the `DurableLoans.LoanOfficerNotificationService` project.
1. `dotnet run` the `DurableLoans.Web` project.

## Run it

1. On the first link, provide your name and a loan amount. Note that this UI is written in Blazor. If you want to call the exchange rate service to try it out, it will call it directly from the client. Yay Blazor!
1. Submit the loan.
1. It will bounce you to the dashboard. 
1. Watch the magic. 