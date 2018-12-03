# Minion Public Scripts
## A collection of scripts for FiveM

This is a collection of utility and mechanic scripts for FiveM. Below you will find the list of scripts and instructions.

* **mc_secpass**

   This is a utility to introduce cryptographically secure password hashing to FiveM. It uses PBKDF2 to hash passwords to be stored in a database. It does not do this on its own, its purpose is to provide the hashing and verification for other modules to utilize. It contains two utility method exports:

   * hashPassword - creates a secure password hash that can later be verified if stored to a database - returns a string
   * verifyPassword - verifies a password guess against a stored hashed password - returns a bool

   To install mc_secpass, simply dowload the zip and extract to your resources folder.
   In your server.cfg, add:

   `start mc_secpass`

   For any module that you want to use this in, make sure in the __resource.lua you add:
   ```lua 
   dependencies {
      'mc_secpass'
   }
   ```
   
   **Usage**
   
   * exports['mc_secpass']:hasPassword(password)
   * exports['mc_secpass']:verifyPassword(guess, password)
   
   ![Example](https://i.ibb.co/92QCYY0/secpassexamplecode.png "Example Code")
   
   ![Example Result](https://i.ibb.co/6HgMXz2/sectestpic.png "Example Code Result")
