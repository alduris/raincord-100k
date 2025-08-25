import os
import math

COLS = os.get_terminal_size().columns - 1
BASE_PATH = "./mod/world/100k-rooms/"
ALL_REGIONS = ["CC", "CL", "DM", "DS", "GW", "HI", "HR", "LC", "LF", "LM", "MS", "OE", "RM", "SB", "SH", "SI", "SL", "SS", "SU", "UG", "UW", "VS", "WARA", "WARB", "WARC", "WARD", "WARE", "WARF", "WARG", "WAUA", "WBLA", "WDSR", "WGWR", "WHIR", "WORA", "WPTA", "WRFA", "WRFB", "WRRA", "WRSA", "WSKA", "WSKB", "WSKC", "WSKD", "WSSR", "WSUR", "WTDA", "WTDB", "WVWA"]

regions = dict()
authors = dict()
missing = set(ALL_REGIONS)

screen_count = 0
room_count = 0

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

for file in os.listdir(BASE_PATH):
	if file.endswith("_credits.txt"):
		room_count += 1
		# region
		region = file.split("_")[1]
		if region not in ALL_REGIONS:
			print("WARNING: '" + region + "' not in valid region list!")
		else:
			missing.discard(region)
			incr_count(regions, region)
		# author
		with open(os.path.join(BASE_PATH, file), "r", encoding="utf8") as f:
			for line in f:
				if ": " in line:
					incr_count(authors, line.split(": ")[1].strip())
				else:
					incr_count(authors, line.strip())
	elif file.endswith(".png"):
		screen_count += 1

# print time
print()  # initial space
print("ROOM COUNT: " + str(room_count) + " / SCREEN COUNT: " + str(screen_count))
print()  # another space lol

# vars
overratios = [1, 1, 3]
arrayorder = [sorted(dict_to_list(regions), key=lambda x:x.upper()), sorted(missing, key=lambda x:x.upper()), sorted(dict_to_list(authors), key=lambda x:x.upper())]
innerratios  = [[1,1], [1,1,1], [1,1,1]]
subseps = [" ", " ", " "]

# header
print_cols(["SUBMISSIONS", "NOT TAKEN YET", "AUTHORS"], overratios, COLS, " | ")
print_hr("-", overratios, COLS, "-+-")

# body
rowcount = calc_row_count(arrayorder, [len(x) for x in innerratios])
for i in range(rowcount):
	print_subcols([get_strs(strs, i, len(ratios)) for strs, ratios in zip(arrayorder, innerratios)], overratios, innerratios, COLS, " | ", subseps)

input("\nPress enter to continue...")