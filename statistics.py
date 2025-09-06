import os
import math

COLS = os.get_terminal_size().columns - 1
BASE_PATH = "./mod/world/100k-rooms/"
ALL_REGIONS = ["CC", "CL", "DM", "DS", "GW", "HI", "HR", "LC", "LF", "LM", "MS", "OE", "RM", "SB", "SH", "SI", "SL", "SS", "SU", "UG", "UW", "VS", "WARA", "WARB", "WARC", "WARD", "WARE", "WARF", "WARG", "WAUA", "WBLA", "WDSR", "WGWR", "WHIR", "WORA", "WPTA", "WRFA", "WRFB", "WRRA", "WRSA", "WSKA", "WSKB", "WSKC", "WSKD", "WSSR", "WSUR", "WTDA", "WTDB", "WVWA"]

regions = dict()
authors = dict()
missing = set(ALL_REGIONS)
screens_regions = dict()
screens_authors = dict()
user_to_author = dict()

counted_credits = set()
counted_settings = set()

screen_count = 0
room_count = 0
real_room_count = 0

def incr_count(dct, key):
	if key not in dct:
		dct[key] = 1
	else:
		dct[key] += 1

def fill_space(str, col):
	return str + (" " * (col - len(str)))

def split_space(ratios, space, seplen):
	space = space - (len(ratios) - 1) * seplen
	total = sum(ratios)
	spaces = []
	for i in range(len(ratios) - 1):
		spaces.append(round(space * ratios[i] / total))
	spaces.append(space - sum(spaces))  # fill in the rest
	return spaces

def print_cols(strs, ratios, width, separator, end="\n"):
	spaces = split_space(ratios, width, len(separator))
	print_strs = [fill_space(str, space) for str, space in zip(strs, spaces)]
	print(separator.join(print_strs), end=end)

def print_hr(line, ratios, width, separator):
	spaces = split_space(ratios, width, len(separator))
	print(separator.join([line * space for space in spaces]))

def print_subcols(strs, ratios, subratios, width, separator, subseparators):
	first = True
	spaces = split_space(ratios, width, len(separator))
	for substrs, rats, space, subseparator in zip(strs, subratios, spaces, subseparators):
		if not first: print(separator, end="")
		first = False
		print_cols(substrs, rats, space, subseparator, "")
	print()

def calc_row_count(arrs, cols_per):
	m = 0
	for arr, col in zip(arrs, cols_per):
		m = max(m, math.ceil(len(arr) / col))
	return m

def get_strs(arr, iteration, grab_per):
	strs = []
	for i in range(grab_per * iteration, grab_per * (iteration + 1)):
		if i < len(arr):
			strs.append(arr[i])
		else:
			strs.append("")
	return strs

def dict_to_list(d):
	return [str(k) + ": " + str(v) for k,v in d.items()]

def subsort_lists(arr):
	for i in range(len(arr)):
		arr[i] = sorted(arr[i], key=lambda x: str(x).upper())

# count up files
for file in os.listdir(BASE_PATH):
	if file.endswith("_credits.txt"):
		room_count += 1
		counted_credits.add(file[:-12])
		# region
		region = file.split("_")[1]
		if region not in ALL_REGIONS:
			print("WARNING: '" + region + "' not in valid region list!")
		else:
			missing.discard(region)
			incr_count(regions, region)
		# author
		author = ""
		with open(os.path.join(BASE_PATH, file), "r", encoding="utf8") as f:
			for line in f:
				if ": " in line:
					author = line.split(": ")[1].strip()
				else:
					author = line.strip()
		incr_count(authors, author)
		user = file.split("_")[2]
		if user not in user_to_author:
			user_to_author[user] = author
		elif user_to_author[user] != author:
			print("WARNING: author mismatch ('" + user_to_author[user] + "' != '" + author + "' for key '" + user + "')")
	elif file.endswith("_settings.txt"):
		real_room_count += 1
		counted_settings.add(file[:-13])
	elif file.endswith(".png"):
		screen_count += 1
		region = file.split("_")[1]
		user = file.split("_")[2]
		incr_count(screens_regions, region)
		incr_count(screens_authors, user)

# double check we didn't miss any
if room_count != real_room_count:
	print("ERROR: settings count and credits count DO NOT MATCH!!!")
	print("Off factor: " + str(real_room_count - room_count))
	print("Affected rooms: " + ", ".join((counted_credits | counted_settings) - (counted_credits & counted_settings)))
	input("\nPress enter to continue...")
	exit()

# update screens_authors
temp = dict()
for user, count in screens_authors.items():
	if user in user_to_author:
		author = user_to_author[user]
		if author in temp:
			dups = [u for u, a in user_to_author.items() if a == author]
			print("WARNING: duplicate author '" + author + "' found in keys: '" + ("', '".join(dups)) + "'")
			temp[author] += count
		else:
			temp[author] = count
	else:
		print("WARNING: unknown user '" + user + "'")
screens_authors = temp
temp = None

# print overall statistics
print()  # initial space
print("ROOM COUNT: " + str(room_count) + " / SCREEN COUNT: " + str(screen_count))
print()  # another space lol

# vars
overratios = [1, 1, 5]
arrayorder = [dict_to_list(regions), missing, dict_to_list(authors)]
innerratios  = [[1,1], [1,1], [1,1,1]]
subseps = [" ", " ", " "]
subsort_lists(arrayorder)

# header
print_hr("-", overratios, COLS, "-+-")
print_cols(["SUBMISSIONS", "NOT TAKEN YET", "AUTHORS"], overratios, COLS, " | ")
print_hr("-", overratios, COLS, " | ")

# body
rowcount = calc_row_count(arrayorder, [len(x) for x in innerratios])
for i in range(rowcount):
	print_subcols([get_strs(strs, i, len(ratios)) for strs, ratios in zip(arrayorder, innerratios)], overratios, innerratios, COLS, " | ", subseps)

print_hr("-", overratios, COLS, "-+-")

# another section! init vars
overratios = [1, 4]
arrayorder = [dict_to_list(screens_regions), dict_to_list(screens_authors)]
innerratios = [[1,1], [1,1,1,1]]
subseps = [" ", " "]
subsort_lists(arrayorder)

# new header
print_hr("-", overratios, COLS, "-+-")
print_cols(["SCREENS PER REGION", "SCREENS PER AUTHOR"], overratios, COLS, " | ")
print_hr("-", overratios, COLS, " | ")

# new body
rowcount = calc_row_count(arrayorder, [len(x) for x in innerratios])
for i in range(rowcount):
	print_subcols([get_strs(strs, i, len(ratios)) for strs, ratios in zip(arrayorder, innerratios)], overratios, innerratios, COLS, " | ", subseps)

print_hr("-", overratios, COLS, "-+-")

# stay on screen until user presses enter to close
input("\nPress enter to continue...")
