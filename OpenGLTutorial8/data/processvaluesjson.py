import json

file = open('values.json','r')
data = file.read()
file.close()

values = json.loads(data)
newValc = {}

for i in range(len(values["Values"])):
    temp = values["Values"][i]
    
    sPos = temp["sPos"]
    p = sPos[sPos.find("[")+1:sPos.find("]")]
    p = p.split(' ')
    
    for pos in range(len(p)):
        p[pos] = float(p[pos])

    k = i+1
    newValc[k] = {}
    newValc[k]["sPos"] = p
    
    detectorPos = temp["detectorPos"]
    p = detectorPos[detectorPos.find("[")+1:detectorPos.find("]")]
    p = p.split(';')

    BottomLeft = p[0].split(' ')
    BottomRight = p[1].split(' ')
    TopLeft = p[2].split(' ')
    TopRight = p[3].split(' ')

    q = BottomLeft
    for pos in range(len(q)):
        q[pos] = float(q[pos])
        
    q = BottomRight
    for pos in range(len(q)):
        q[pos] = float(q[pos])
        
    q = TopLeft
    for pos in range(len(q)):
        q[pos] = float(q[pos])
        
    q = TopRight
    for pos in range(len(q)):
        q[pos] = float(q[pos])

    newValc[k]["dBL"] = BottomLeft
    newValc[k]["dBR"] = BottomRight
    newValc[k]["dTL"] = TopLeft
    newValc[k]["dTR"] = TopRight

file = open("processedVals.json",'w')
file.write(json.dumps(newValc))
file.close()

