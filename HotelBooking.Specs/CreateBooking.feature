Feature: Create Booking
As a hotel manager
I want to create bookings for available rooms
So that customers can reserve rooms for specific dates

    Background:
        Given the current date is today

    Scenario: Successfully create a booking when a room is available
        Given there is an available room
        When I create a booking with start date tomorrow and end date two days from now
        Then the booking should be created successfully

    Scenario: Fail to create a booking with a past start date
        Given there is an available room
        When I create a booking with start date yesterday and end date tomorrow
        Then an error should be thrown with message "The start date cannot be in the past or later than the end date."

    Scenario: Fail to create a booking when end date is before start date
        Given there is an available room
        When I create a booking with start date tomorrow and end date today
        Then an error should be thrown with message "The start date cannot be in the past or later than the end date."

    Scenario: Fail to create a booking when no rooms are available
        Given all rooms are booked from tomorrow to two days from now
        When I create a booking with start date tomorrow and end date two days from now
        Then the booking should not be created