## Web Crawler

To crawl www.zaubacorp.com to understand Director relationships.

It has a user interface, where the user can enter the url and depth to be traversed.

## Inputs:

1. Starting URL - You will get a starting URL which is the zaubacorp page for a company.
2. Depth - a number between 1 and 5.

## Sample Inputs
 URL=https://www.zaubacorp.com/company/DR-REDDY-S-LABORATORIES-LTD/L85195TG1984PLC004507 and Depth = 5


## Process

1. Visit the URL and collect director information which is present on that page such as this under Director Details and add this to a 
dataframe with columns  URL, DIN, Director Name, Designation, Appointment Date, Search Depth.

2. If Depth is 1 - save dataframe to a csv and stop

3. If Depth is > 1 - Loop through each director - Click into view other directorships for each director - for instance the second person
in the list above has a list of other companies listed

4. Loop through each company and collect its director details in turn adding to the original dataframe

5. Decrement Depth by 1 and go back to step 2.

6. After extracting all the director-company relationships, it puts all the director-company relationships in a NEO4J graph. 

# Assumptions

1. Once a company's url is crawled and if it gets encountered further, it is not crawled again.

2. The process includes saving data in a CSV file. Replace the path of that file in GenerateCSV and Connect method of ValuesDL.cs with 
some local machine path.

## Contributing
Pull requests are welcome. For major changes, please we can discuss what you would like to change.
