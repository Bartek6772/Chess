import os 

# Przydatny program do zmiany dużych liter na małe i oddzielenia wyrazów znakiem "_"

for entry in os.scandir("./Database/"):  
    if entry.is_file():
        name = "_".join([(p[0].lower() + p[1:]) for p in entry.name.split(" ")])
        os.rename(entry, name)
