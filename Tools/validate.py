"""Static prototype checks. No RimWorld installation or Python packages required."""
from pathlib import Path
import re
import struct
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
for path in root.rglob('*.xml'):
    ET.parse(path)
ET.parse(root / 'Source/Minisplits/Minisplits.csproj')
defs = ET.parse(root / 'Defs/ThingDefs/Minisplits.xml').getroot()
buildings = [node for node in defs if node.get('Abstract') != 'True']
assert [n.findtext('defName') for n in buildings] == ['Minisplits_Small', 'Minisplits_Large']
base = defs.find("ThingDef[@Name='MinisplitsBase']")
assert base.findtext('building/isEdifice') == 'false'
assert base.findtext('building/canPlaceOverWall') == 'true'
assert base.findtext('clearBuildingArea') == 'false'
assert base.findtext('tickerType') == 'Rare'
source = '\n'.join(p.read_text() for p in (root / 'Source').rglob('*.cs'))
keys = {n.tag for n in ET.parse(root / 'Languages/English/Keyed/Minisplits.xml').getroot()}
assert set(re.findall(r'"(Minisplits_[A-Za-z]+)"', source)) <= keys
classes = set(re.findall(r'class\s+(\w+)', source))
for name in ['Building_Minisplit', 'PlaceWorker_Minisplit', 'MinisplitProperties']:
    assert name in classes
for node, watts, capacity, width in zip(buildings, [350, 700], [21, 42], [128, 256]):
    power = node.find("comps/li[@Class='CompProperties_Power']")
    temp = node.find("comps/li[@Class='Minisplits.MinisplitProperties']")
    assert int(power.findtext('basePowerConsumption')) == watts
    assert float(temp.findtext('energyPerSecond')) == -capacity
    assert float(temp.findtext('heatingEnergyPerSecond')) == capacity
    assert 0 < float(temp.findtext('lowPowerConsumptionFactor')) < 1
    path = root / 'Textures' / (node.findtext('graphicData/texPath') + '.png')
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n'
    assert struct.unpack('>II', data[16:24]) == (width, 128)
    assert data[25] in (4, 6), 'Textures must include transparency'
assert 'GenTemperature.PushHeat' in source
assert 'base.TickRare();' in source
print('PASS: XML/project syntax, two buildings, wall overlay flags, balance, PNGs, classes, translations.')
print('C# compilation and in-game behavior remain unverified.')
