import os

credits = dict()
renames = dict()
with open("100K_credits_map.txt", "r", encoding="utf8") as f:
	for line in f:
		line = line.strip()
		if " : " in line:
			i = line.index(" : ")
			username = line[:i]
			display = line[i+3:]
			credits[username] = display

do_repeats = input("Do repeats? (y/N)").lower() == "y"

print("ACTIONS: NEXT, RENAME, SET, QUIT")

for file in os.listdir("./mod/text/text_eng/"):
	start = file.index("_")
	end = file.index("_", start+1)
	name = file[start+1:end]
	if name != name.lower():
		os.rename("./mod/text/text_eng/" + file, "./mod/text/text_eng/" + file[:start+1] + name.lower() + file[end:])
	name = name.lower()
	if name in renames:
		new_name = renames[name]
		while new_name in renames:
			new_name = renames[new_name]
		if new_name != name:
			os.rename("./mod/text/text_eng/" + file, "./mod/text/text_eng/" + file[:start+1] + new_name + file[end:])
			name = new_name
	inp = ""
	if not do_repeats and name in credits:
		continue
	while inp.lower() != "next" and (not name in credits or do_repeats):
		print()
		print("CURRENT: " + name + " (" + file + ")")
		if name in credits:
			print("DISPLAY NAME: " + credits[name])
		else:
			print("NAME NOT IN DISPLAY CREDITS!")
		inp = input("Action: ").lower()
		if inp == "quit":
			exit()
		elif inp == "next":
			break
		elif inp == "set":
			new_name = input("New name: ")
			credits[name] = new_name
		elif inp == "rename":
			new_name = input("New username: ").lower()
			if new_name != name:
				os.rename("./mod/text/text_eng/" + file, "./mod/text/text_eng/" + file[:start+1] + new_name + file[end:])
				renames[name] = new_name
				if name in credits:
					credits[new_name] = credits[name]
					credits.pop(name)
				if new_name in renames:
					renames.pop(new_name)
				name = new_name

print(credits)

writemap = list()
for k,v in credits.items():
	writemap.append(k + " : " + v + "\n")
writemap = sorted(writemap)
with open("100K_credits_map.txt", "w", encoding="utf8") as f:
	for item in writemap:
		f.write(item)

input("Done!")