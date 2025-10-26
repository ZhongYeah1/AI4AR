import re
def main(): 
    with open ('selected_points.txt', 'r') as f: 
        content = f.read()
    
    pattern = r'#(\d+)'
    indices = re.findall(pattern, content)

    with open('index.txt', 'w') as f:
        for idx in indices: 
            f.write(idx + '\n')

if __name__ == "__main__": 
    main()