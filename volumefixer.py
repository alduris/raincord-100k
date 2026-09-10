import os
import re

COLS = os.get_terminal_size().columns - 1
BASE_PATH = "./mod/world/100k-rooms/"

for file in os.listdir(BASE_PATH):
	if file.lower().endswith("_settings.txt"):
		print(file)
		needs_overwriting = False;
		lines = []
		with open(os.path.join(BASE_PATH, file), "r", encoding="utf8") as f:
			for line in f:
				if line.startswith("Triggers:"):
					index = 0
					while True:
						identifier = "<eA>vol<eB>"
						index = line.find(identifier, index)
						if index > -1:
							index += len(identifier)
							end_index = line.find("<eA>", index)
							num = float(line[index:end_index])
							div = 1
							if num > 0.7:
								div = 2.5
							elif num > 0.55:
								div = 2
							elif num > 0.4:
								div = 1.5
							if div != 1:
								needs_overwriting = True
								line = line[:index] + str(round(num / div, 6)) + line[end_index:]
						else:
							break
				lines.append(line)
		if needs_overwriting:
			with open(os.path.join(BASE_PATH, file), "w", encoding="utf8") as f:
				f.write("".join(lines))