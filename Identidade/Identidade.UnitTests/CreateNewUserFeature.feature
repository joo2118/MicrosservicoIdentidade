Feature: CreateNewUser
	Tests for the UserRepository class
	On Create method

Scenario: Create Successfully
	Given the IdGenerator returning "USR_GUID" for GenerateId("USR")
	And the SecureHashAlgorithm returning "EncriptedPassword" for Create("password")
	When I input a new user and the password "password"
	Then the username and the password are validated
	And the user is created successfully

Scenario: Create with Invalid Password Throws AppException
	Given the passwordValidator throwing AppException for Validate("password")
	When I input a new user and the password "password"
	Then an AppException is thrown

Scenario: Create with Invalid UserName Throws AppException
	Given the userValidator throwing AppException for Validate("new.user")
	When I input a new user with username "new.user"
	Then an AppException is thrown

Scenario: Create with User Groups
	Given the IdGenerator returning "USR_GUID" for GenerateId("USR")
	And there exists user groups with IDs UGR_GUIDGroup1 and UGR_GUIDGroup2
	When I input a new user with the groups 1 and 2
	Then the user is created with the groups 1 and 2

Scenario: Create with User Substitution
	Given the IdGenerator returning "USR_GUID" for GenerateId("USR")
	And there exists users with IDs USR_GUIDUser1 and USR_GUIDUser2
	When I input a new user with the substitutes 1 and 2
	Then the user is created with the substitutes 1 and 2
