### Title
Implementing system and business parameters management in backend and admin

### Description
I need a parameter management for system wide parameters and business wide parameters. For instance cache timeout is an example to system parameters and anonymous cart duration is an example to business management parameters.

### System Admin User Story
As a system administreator
I want to manage system wide parameters
So that i manage system without making ay changes in code

### Business Admin User Story
As a b2b site owner
I want to manage some of rules and limits via admin
So that i can manage business and site according to needs

### Requirements
-[] There will be two parameter type. System parameters and business parameters
-[] Parameter name format "xxxx.yyyy"
    - xxxx -> specifies module or base of parameter. If it's about catalog management it will be something like "catalog.product.default-status"
-[] Parameters should be managed via admin ui
