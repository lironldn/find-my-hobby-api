Feature: Course search railguards

Scenario: Accept valid course search input
    Given the Find My Hobby API is available for course search
    When I request a course search with valid input
    Then the course search response status code should be 200
    And the course search response should contain relevant results

Scenario: Reject overlong hobby descriptions
    Given the Find My Hobby API is available for course search
    When I request a course search with an overlong hobby description
    Then the course search response status code should be 400
    And the course search response detail should be "Hobby description must be 200 characters or less."

Scenario: Reject invalid postcodes
    Given the Find My Hobby API is available for course search
    When I request a course search with an invalid postcode
    Then the course search response status code should be 400
    And the course search response detail should be "UK postcode must be a valid UK postcode."

Scenario: Reject prompt injection attempts
    Given the Find My Hobby API is available for course search
    When I request a course search with an injected hobby description
    Then the course search response status code should be 400
    And the course search response detail should be "Hobby description contains instructions unrelated to hobby search."
