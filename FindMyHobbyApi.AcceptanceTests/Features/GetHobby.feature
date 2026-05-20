Feature: Get hobby suggestions

Scenario: Get hobby suggestions successfully
    Given the Find My Hobby API is available
    When I request hobby suggestions
    Then the response status code should be 200
    And the response should contain 5 hobbies
    And each hobby should have a name