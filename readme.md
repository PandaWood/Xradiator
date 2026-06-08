
Xradiator is a cross-platform Avalonia app (ported from the WPF app Cradiator) that displays a summary of Continuous Integration (CI) project statuses. It is basically an _Information Radiator_ for CI.

Xradiator supports two modes of CI integration:

- **TeamCity native REST API** — rich data including build breakers, categories, and server names
- **CCTray XML feed** — basic status (name, activity, last build status/time) from any CI server that exposes the [CCTray v1](https://cctray.org/v1/) format. This includes [TeamCity](https://www.jetbrains.com/teamcity) (built-in), [Jenkins](https://plugins.jenkins.io/cctray-xml/) (via plugin), CruiseControl.NET, CruiseControl (Java), and any other CCTray-compatible server.

Xradiator is most suited to display on a dedicated monitor where it is visible to developers working on a project.
There are various screen layouts or **skins** to choose from:

#### Stack Skin
Projects are arranged in a stack (top-to-bottom). A countdown of how many seconds to go before refreshing can also be shown (this can be switched off).

#### Grid Skin
Projects are arranged in a grid.

#### StackPhoto Skin
Shows an image alongside a broken project (based on the breaker's username). Only 1 breaker is shown.

#### Color Chart
| Color | Status |
|-------|--------|
| Green | Success/Normal |
| Red | Failure/Exception/Error |
| Yellow | Building |
| White | Unknown |

### Config documentation

1. __Polling Frequency__ - In seconds. Default is 30

2. __Views/view__ - Each 'view' can be defined in this xml section 
 * If more than 1 view is specified, then the view is switched (on a rotation cycle) at each poll interval.
 * Each view contains a url & other base settings as documented below
    
3. __view/url__ - The CI server URL. Any URL not matching the TeamCity REST pattern is treated as a generic CCTray feed.

    **TeamCity (native REST API — rich data)**
    * `http://localhost:8111/guestAuth/app/rest` — guest auth
    * `http://localhost:8111/app/rest?token=YOUR_TOKEN` — access token auth

    **TeamCity (CCTray feed — basic status only, no breakers/category/server)**
    * `http://localhost:8111/guestAuth/app/rest/cctray/projects.xml`

    **Other CCTray-compatible servers (basic status only)**
    * Jenkins: `http://localhost:8080/cc.xml` (requires the [CCtray XML plugin](https://plugins.jenkins.io/cctray-xml/))
    * CruiseControl.NET: `http://localhost/ccnet/XmlStatusReport.aspx`
    * CruiseControl (Java): `http://localhost:8080/cctray.xml`
    * Any other server exposing the [CCTray v1](https://cctray.org/v1/) format

    **Debug mode** — prepend the URL with the word `debug` to use a local `DummyProjectStatus.xml` file instead of connecting to a server (useful for testing/screenshots)
                                                                                                                                                                                    
3. __multi-url__ - URL can be split (using space as a delimiter), to refer to multiple urls. 
     - eg value="http://url1 http://url2"
     - All project data is collected into one screen output 
     - DEBUG MODE (ie prepending word 'debug' to URL) overrides the multiurl feature; no multiurls are read if debug is on
	
4. __view/project-regex__ - RegEx used to filter which projects are included (by name)
    * Defaults to ".*" (even if config is "")
	
5. __view/category-regex__ - as for ProjectNameRegEx but filters by category name

6. __view/server-regex__ - as for ProjectNameRegEx but filters by server name

7. __view/name__ - the name for this view, will be shown when ShowOnlyBroken = true and there are no broken projects

* __view/showServerName__ - shows the servername below the project name if true, handy if you monitor multiple servers with identical project names

* __view/showOutOfDate__ - 'true' or 'false'. When true, projects whose last build is older than __outOfDateDifferenceInMinutes__ are flagged as out of date

* __view/outOfDateDifferenceInMinutes__ - the age (in minutes) after which a project is considered out of date (only applies when __showOutOfDate__ is true)

* __view/skin__ - Currently 3 choices 
    1. Grid - arranged in a grid format
    2. Stack - arranged in a stack (ie top-to-bottom listbox type) format
    3. StackPhoto - same as Stack but shows an image of the build breaker as well as text 
    
    
    last/first listed - is the last/first person to commit while build is broken - where 'last/first' 
is dependent on the setting 'BreakerGuiltStrategy' (below) - and not necessarily honored by the server producing XML, so use with a grain of salt


* __photo/image__ - requires a sub-folder named 'images' (relative to the folder in which
the app resides) with a JPEG ([username].jpg) corresponding to each username
 - The JPEG file must be named using the username - eg bsimpson requires a filepath 'images/bsimpson.jpg'
 - If a file/photo does not exist for a user, everything will still work as normal (ie it's not considered an error)
	
* __ShowCountdown__ - 'true' or 'false' (case insensitive) ,so { True False TRUE FALSE } are all valid
    - Shows a clock that counts down the 'time to go' before refreshing the screen (updated approx every second)

* __ShowProgress__ - 'true' or 'false', whether to show a progress indicator while build data is being fetched

* __PlaySounds__ - true/false, whether to play sounds on events (described below) (.WAV files only)

* __BrokenBuildSound__ - the filename (without path) of the .WAV file
    - The file is assumed to be in a sub-folder named 'sounds' (relative to the folder in which the app resides) 
    - A 'BrokenBuildSound' plays in response to a project that starts 1) not Broken (FAILURE|EXCEPTION) followed by 2) Broken  
	
* __FixedBuildSound__ - as for BrokenBuildSound, but plays in the case of 1) Broken (FAILURE|EXCEPTION) followed by 2) SUCCESS

* __usernames__ - a section for mapping usernames to real or full-length names, used when displaying the build breaker
 - eg if your username is 'jbloggs' you can map it to 'Johann Bloggs' via this config so the full name is shown instead of the username
 - A username of 'jsmith' is mapped to 'John Smith' by default, in app.config, to make the format required obvious
					
* __BreakerGuiltStrategy__ - 'First' or 'Last' 
 - How to determine the 'Breaker', is the 'First' build breaker always guilty?, or do subsequent committers ('Last' breaker) assume guilt when they commit over a breaking build?
