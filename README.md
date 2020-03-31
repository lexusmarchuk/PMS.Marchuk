# Project Management System :unicorn:

## How to Setup
You need to create the database using the **update-database** command in the **Package Manager Console** from **Visual Studio**, as below.
```
PM>update-database –verbose
```

**Note:** _change connection string frist_ :warning:

## How to Use
Once you run web service you will see **swagger/index.html** with all entpoints and ability to execute them all.

### Available API endpoints
API | Description
------------ | -------------
GET ​/api​/Project​/report | Generates Excel report
GET ​/api​/Project | Get Project by code
POST ​/api​/Project | Create Project
POST ​/api​/Task | Create Task
GET ​/api​/Task | Get task by Id
POST ​/api​/Task​/subtask | Create sub-task
POST ​/api​/Task​/start | Changes Task state to _InProgress_
POST ​/api​/Task​/finish | Changes Task state to _Completed_
