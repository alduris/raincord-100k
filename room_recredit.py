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

BASE_PATH = "./mod/world/100k-rooms/"
room_groups = dict()
for file in os.listdir(BASE_PATH):
	if file.endswith(".txt") and file[file.rindex("_")+1:-4].isdigit():
		room_groups[file[:-4]] = list()
	elif file.endswith(".txt") and not file.endswith("_credits.txt") and not file.endswith("_settings.txt"):
		print("INFO: weird file ending found. '" + file + "'")
for file in os.listdir(BASE_PATH):
	for key in room_groups.keys():
		if file.startswith(key):
			room_groups[key].append(file)
			break

do_repeats = input("Do repeats? (y/N)").lower() == "y"

print("ACTIONS: NEXT, RENAME, SET, QUIT")

try:
	for room, files in room_groups.items():
		# Fix name
		start = room.index("_", 6)
		end = room.rindex("_")
		name = room[start+1:end].lower()
		if name in renames:
			new_name = renames[name]
			while new_name in renames:
				new_name = renames[new_name]
			name = new_name
		num = int(room[end+1:])
		new_room = room[:start+1] + name + "_" + str(num)
		# Rename if necessary
		if room != new_room:
			for i in range(len(files)):
				file = files[i]
				file_rename = new_room + file[len(room):]
				os.rename(BASE_PATH + file, BASE_PATH + file_rename)
				files[i] = file_rename
			room = new_room
		# Ok yippee
		inp = ""
		if not do_repeats and name in credits:
			continue
		while inp.lower() != "next":
			print()
			print("CURRENT: " + name + " (" + room + ")")
			temp_name = ""
			credits_path = BASE_PATH + new_room + "_credits.txt"
			if os.path.exists(credits_path):
				with open(credits_path, "r", encoding="utf8") as f:
					temp_name = f.read().strip()
				os.remove(credits_path)
				files.remove(new_room + "_credits.txt")
			do_inp = True
			if name in credits:
				if temp_name != "" and temp_name != credits[name]:
					print("CONFLICTING NAMES!")
					print("  Credits file name: " + temp_name)
					print("  Cached credits name: " + credits[name])
					inp = "set"
					do_inp = False
				else:
					print("DISPLAY NAME: " + credits[name])
			else:
				if temp_name == "":
					print("NAME NOT IN DISPLAY CREDITS AND NO CREDITS FILE!")
					inp = "set"
					do_inp = False
				else:
					credits[name] = temp_name
					print("DISPLAY NAME: " + credits[name])
			if do_inp:
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
					new_room = room[:start+1] + new_name + "_" + str(num)
					for file in files:
						os.rename(BASE_PATH + file, BASE_PATH + new_room + file[len(room):])
					renames[name] = new_name
					if name in credits:
						credits[new_name] = credits[name]
						credits.pop(name)
					if new_name in renames:
						renames.pop(new_name)
					name = new_name
					room = new_room
finally:
	print(credits)

writemap = list()
for k,v in credits.items():
	writemap.append(k + " : " + v + "\n")
writemap = sorted(writemap)
with open("100K_credits_map.txt", "w", encoding="utf8") as f:
	for item in writemap:
		f.write(item)