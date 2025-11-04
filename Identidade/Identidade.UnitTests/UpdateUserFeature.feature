Feature: UpdateUser
	Tests for the UserRepository class
	On Update Method

Scenario: Update User Changes LastUpdatedAt Property
	Given an existing user
	When I update the user
	Then the LastUpdatedAt proterty is updated

Scenario: Update Password Changes PasswordHistory
	Given an existing user with passwordHash "EncriptedPassword"
	When I update the passwordHash for "NewEncriptedPassword"
	Then the password and the passwordHistory are updated