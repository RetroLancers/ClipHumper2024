import sys

from twitchdl.commands.download import get_clip_authenticated_url

if __name__ == "__main__":
 
    url = get_clip_authenticated_url(sys.argv[1], "source")
    print("_start_")
    print(url)
    print("_stop_")
