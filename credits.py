import os

credits = dict()
with open("./mod/100K_credits_map.txt", "r", encoding="utf8") as f:
	for line in f:
		line = line.strip()
		if " : " in line:
			i = line.index(" : ")
			username = line[:i]
			display = line[i+3:]
			credits[username] = display

PEARL_PATH = "./mod/text/text_eng/"
ROOMS_PATH = "./mod/world/100k-rooms/"

pearls = set()
rooms = set()

for file in os.listdir(PEARL_PATH):
	split = file.split("_")
	if len(split) > 1:
		pearls.add(split[1])

for file in os.listdir(ROOMS_PATH):
	split = file.split("_")
	if len(split) > 2:
		rooms.add(split[2])

print("Pearls:")
sorted_pearls = sorted([credits[a] for a in pearls], key=lambda x: str(x).upper())
for author in sorted_pearls:
	print(author)
print()
print("Rooms:")
sorted_rooms = sorted([credits[a] for a in rooms], key=lambda x: str(x).upper())
for author in sorted_rooms:
	print(author)

print()
input("Press enter to continue...")