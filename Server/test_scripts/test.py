import csv

with open("requests_20250822_153908.csv", newline='', encoding='utf-8-sig') as f:
    reader = csv.reader(f)
    header = next(reader)
    print(header)