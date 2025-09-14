Task Manager (.NET Backend Application)
This application allows to control the process flow of a project.
Admin can add projects, add users, assign users to projects, add stages to projects, and assign users to stages.
User can do commits to their stages and look through previous ones.
Users without the admin role cannot access admin services.
Admin Access
To access the admin level of the application you should use the AddAdmin service, which will register an admin-level user that may operate with the systems described above.
Authentication and Authorization
The application has authentication and authorization.
After login you will receive a JSON Web Token (JWT).
Copy this token, click the lock icon in Swagger, and paste it there to be logged in.
Then you will be able to perform actions according to your access level.
Note: Email verification and password reset were planned but not implemented due to API costs and slow trial services.
Running the Application
The project is containerized so it can work on other machines.
Start the containers.
Open in your browser:
http://localhost:5000/swagger
Register a new user (username, email, password).
Login and authorize with the JWT as described above.
Project status, porject type, roles are described as enums
Project status: 0 - dvelopment, 1 - deployment, 2 - maintance, 3 - planning, 4 - complted, 5 - cancelled.
Project type: 0 - PetProject, 1 - small project, 2 - enterprise level.
Roles: 0 -manager, 1 - developer, 2 - tester, 3 - TeamLead, 4 -DevOps, 5 - designer (Manager and TeamLead are considered to be admin roles).
