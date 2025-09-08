import os
import math

COLS = os.get_terminal_size().columns - 1
BASE_PATH = "./mod/world/100k-rooms/"
ALL_REGIONS = ["CC", "CL", "DM", "DS", "GW", "HI", "HR", "LC", "LF", "LM", "MS", "OE", "RM", "SB", "SH", "SI", "SL", "SS", "SU", "UG", "UW", "VS", "WARA", "WARB", "WARC", "WARD", "WARE", "WARF", "WARG", "WAUA", "WBLA", "WDSR", "WGWR", "WHIR", "WORA", "WPTA", "WRFA", "WRFB", "WRRA", "WRSA", "WSKA", "WSKB", "WSKC", "WSKD", "WSSR", "WSUR", "WTDA", "WTDB", "WVWA"]

credits = dict()
renames = dict()
with open("./mod/100K_credits_map.txt", "r", encoding="utf8") as f:
	for line in f:
		line = line.strip()
		if " : " in line:
			i = line.index(" : ")
			username = line[:i]
			display = line[i+3:]
			credits[username] = display

regions = dict()
authors = dict()
screens_regions = dict()
screens_authors = dict()

screen_count = 0
room_count = 0

def incr_count(dct, key):
	if key not in dct:
		dct[key] = 1
	else:
		dct[key] += 1

def max_cols(strs, seplen):
	m = 0
	for s in strs:
		m = max(len(s) + seplen, m)
	return (COLS + seplen) // m

def fill_space(str, col):
	return str + (" " * (col - len(str)))

def print_hr(fill):
	print(fill * (COLS // len(fill)))

def print_cols(strs, cols, separator):
	width = (COLS - len(separator) * (cols - 1)) // cols
	print_strs = [fill_space(str, width) for str in strs]
	print(separator.join(print_strs))

def calc_row_count(arr, cols):
	return math.ceil(len(arr) / cols)

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
	if file.endswith(".txt"):
		split = file[:-4].split("_")
		if split[-1].isdigit():
			room_count += 1
			region = split[1]
			author = split[2]
			incr_count(regions, region)
			incr_count(authors, author)
			
	elif file.endswith(".png") and not file.endswith("_light.png"):
		screen_count += 1
		region = file.split("_")[1]
		user = file.split("_")[2].lower()
		incr_count(screens_regions, region)
		incr_count(screens_authors, user)

# print overall statistics
print()  # initial space
print("ROOM COUNT: " + str(room_count) + " / SCREEN COUNT: " + str(screen_count))
print()  # another space lol

# print region counts
print_regions = sorted(dict_to_list(regions), key=lambda x: str(x).upper())
print_regions_cols = max_cols(print_regions, 2)
print_regions_rows = calc_row_count(print_regions, print_regions_cols)
print("REGION COUNTS:")
for i in range(print_regions_rows):
	print_cols(get_strs(print_regions, i, print_regions_cols), print_regions_cols, "  ")

print()

# print author counts
temp = dict()
for author, count in authors.items():
	if author in credits:
		temp[credits[author]] = count
	else:
		print("WARNING: MISSING AUTHOR (" + author + ")")
authors = temp
del temp
print_authors = sorted(dict_to_list(authors), key=lambda x: str(x).upper())
print_authors_cols = max_cols(print_authors, 2)
print_authors_rows = calc_row_count(print_authors, print_authors_cols)
print("AUTHOR COUNTS:")
for i in range(print_authors_rows):
	print_cols(get_strs(print_authors, i, print_authors_cols), print_authors_cols, "  ")

print()

# stay on screen until user presses enter to close
input("\nPress enter to continue...")
