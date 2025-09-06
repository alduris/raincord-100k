import os

BASE_PATH = "./mod/text/text_eng/"
ILLEGAL_CHARS = [("“", '"'), ("”", '"'), ("‘", "'"), ("’", "'"), ("…", "..."), ("—", "---")]
ILLEGAL_REFS = ["little creature", "little one", "little friend", "little archaeologist"]
REF_EXCEPTIONS = ["100K_luniular_2.txt"] # if it is not referring to the player and all other refs have been cleared out of it

found_issue = False

def incr_count(dct, key):
	if key not in dct:
		dct[key] = 1
	else:
		dct[key] += 1

for file in os.listdir(BASE_PATH):
	with open(os.path.join(BASE_PATH, file), "r", encoding="utf8") as f:
		try:
			firstline = f.readline().strip()
			if firstline != "0-" + file[:-4]:
				print("WARNING: '" + file + "' does not contain valid first line!")
				found_issue = True
			contents = f.read().lower()
			for char in ILLEGAL_CHARS:
				if char[0] in contents:
					print("WARNING: '" + file + "' contains illegal char (" + char[0] + ")!")
					found_issue = True
			if file not in REF_EXCEPTIONS:
				for ref in ILLEGAL_REFS:
					if ref in contents:
						print("WARNING: '" + file + "' contains illegal hardcoded player reference! (" + ref + ")")
						found_issue = True
		except Exception as e:
			found_issue = True
			print("ERROR: Encountered exception reading '" + file + "'!")
			print(e)
			break

if not found_issue:
	print("No issues found!")

input("\nPress enter to continue...")