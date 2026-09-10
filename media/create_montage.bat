cd screenshots

dir /B *.png > ../filelist.txt

python -c "import random, sys; lines = open(sys.argv[1]).readlines(); random.shuffle(lines); print(''.join(lines))" ../filelist.txt > ../shuffled.txt

magick montage -geometry 128x72 @../shuffled.txt ../100k_montage.png
