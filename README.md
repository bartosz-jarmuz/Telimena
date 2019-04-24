# Telimena
## Telemetry and Lifecycle Management Engine

### Project overview
Telimena is a platform which is intended to provide a **simple** telemetry and app lifecycle support. 
**Simple** means as little setup & plumbing effort as possible on the *client* app. It allows monitoring application usage, exceptions, logging etc., and managing it's lifecycle - automatic updates, rollouts, centralized settings etc. 
The platform is intended for developers of small-to-medium applications where plugging in any larger and more powerful solutions would be too much effort or cost. It is supposed to be ready to be either hosted internally, as a **private instance**, or used as a **multi-tenant cloud platform**.

### In short, the functional design principles for Telimena are: 
  - **Minimum impact on the *client* app** - Telimena should never break or slow down the application
  - **Minimum effort to plug it in to *client* app** - It should take as few lines of code as possible to set it up
  - **Minimum learning curve for starting using the system** - It shouldn't take more than 15 minutes to understand how Telimena can be used
  - **Maximum number of extra-features that the 'big' apps are equipped with to your app - for free** - Things like automatic updates, release notes, centralized settings management, centralized logging, event analysis, error catching, beta versions and tester users groups, gathering feedback from users, auditing users etc.
  - **Maximum integration friendliness** - It should be as open and pluggable as possible
  
 ### Development principles are:
  - **Do not reinvent the wheel** - unless you want to do stuff differently
  - **Write tests** - preferably integration / end-to-end tests
  - **Do not rush** - it's better to take a bit more time but do something better and more flexible
  
 ### Who is it **__not__** for:
  - Large (enterprise-flagship) applications. If you have a product that has years of history and tens of people working on it full time, then you'll be better off investing a few sprints and creating/connecting something tailored for your needs and budget
  - Web applications. Since you don't need the 'lifecycle' management, you might be better off with something else, like ApplicationInsights
  - Mobile applications - but only because I have no experience with mobile development :)
  
 ### Then who is it for:
  - Enterprises with teams (or individuals) creating lots of **small-to-medium** apps (perhaps for internal productivity?) 
  - Developers who want to **focus on working on their applications' features**, not the infrastructure plumbing that is nice-to-have, but not needed on it's own (who cares if your application can have 3 tiers of beta testing teams with automatic update deployment, if it doesn't do **the work**)
  - Developers who **need a simple 'all-in-one' solution** rather than discovering, learining and plumbing in a separate service for telemetry, separate for filtering errors, separate for supporting application updates, and yet another one for gather 'app feedback' from users
 
 ### Current status
 The project is currently in an alpha stage. The parts that are 'done' are usable (and in use) and stable. There's a **lot** of functionality to be added, as well as some areas that require redesign. Well, it's a journey, not a destination ;)
 
 ### Contributions
 Any kind of contributions or feedback are very welcome. I am looking for collaborators to add more power into the project. Let me know if you want to help it grow.
 
 
  

 # Join the team 
 Do you want to collaborate? Join the project at https://crowdforge.io/projects/585