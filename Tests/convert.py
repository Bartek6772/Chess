list = """
b2b3: 51
g2g3: 51
a3a4: 51
d5d6: 49
g2g4: 51
g2h3: 52
a3b4: 50
d5e6: 55
c3b1: 52
c3d1: 52
c3a2: 52
c3a4: 52
c3b5: 48
e5d3: 52
e5c4: 50
e5g4: 53
e5c6: 50
e5g6: 50
e5d7: 53
e5f7: 52
d2c1: 52
d2e3: 51
d2f4: 52
d2g5: 51
d2h6: 50
e2d1: 53
e2f1: 53
e2d3: 51
e2c4: 49
e2b5: 47
e2a6: 45
a1b1: 52
a1c1: 52
a1d1: 52
a1a2: 52
h1f1: 52
h1g1: 52
f3d3: 51
f3e3: 51
f3g3: 52
f3h3: 52
f3f4: 52
f3g4: 52
f3f5: 54
f3h5: 52
f3f6: 44
e1d1: 52
e1f1: 52
e1g1: 52
e1c1: 52
"""

for line in list.split('\n'):
    if(len(line) < 3):
         continue
    a = line.split(':')[0]
    b = line.split(':')[1]
    print("\"" + a + "\":" + b, end=",")
