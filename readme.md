# Durable Loans Processing

This demo shows how .NET Core can be used to build cloud-native applications than run as microservices, or on serverles platforms, and that support a wide variety of communication patterns including SignalR for real-time communication and gRPC. 

## Project Structure

The solution consists of these projects, each of which serve an individual purpose. 


|Project |Purpose |
|---|---|
|DurableLoans.DomainModel   |A series of classes representing the ontology of the overall application.  |
|DurableLoans.ExchangeRateService   |gRPC service that provides currency exchange rate conversion.|
|DurableLoans.LoanOffice.Inbox      |REST API that receives requests from the Durable Function. The API sends the incoming loan applications into an Azure Storage Queue.|
|DurableLoans.LoanOffice.InboxProcessor      |Worker Service that wathces the inbox queue. When loan applications are dropped onto the queue by the inbox REST API, this project picks them up and saves them to Cosmos, in the `Inbox` container where they await human review.
|DurableLoans.LoanOffice.ToBeApproved   |Back-end service containing a gRPC endpoint that streams loans out to a client used by the loan officer to provide final approval of loan applications.|
|DurableLoans.LoanProcess   |The Azure Function that serves as the back-end for the system.|
|DurableLoans.Web   |The front-end web app.|

## Get it running

1. Configure the `DurableLoans.LoanProcess` project with the correct Azure SignalR Service and Azure Storage connection strings. 
1. `func start` the `DurableProcess.LoanProcess`.
1. `dotnet run` the `DurableLoans.ExchangeRateService` project.
1. `dotnet run` the `DurableLoans.LoanOffice.Inbox` project.
1. `dotnet run` the `DurableLoans.LoanOffice.InboxProcessor` project.
1. `dotnet run` the `DurableLoans.LoanOffice.ToBeApproved` project.
1. `dotnet run` the `DurableLoans.Web` project.

There is also a WPF app that demonstrates how to access the API from a desktop/WPF app. 

