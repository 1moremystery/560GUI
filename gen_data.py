"""Generates Data.sql for the TTRPG Shop database.

Simulates ~3 years of store activity so the data is internally consistent:
- Stock starts at 0, purchase orders restock it, sales draw it down.
- QuantityOnHand in Products = total purchased - total sold.
- Some prices rise on 2025-01-01 and 2026-05-01, so UnitPrice/UnitCost history
  differs from current Products prices.
- New editions launch mid-period; the editions they replace stop being
  reordered and sell through their remaining stock.
"""
import random
from datetime import date, timedelta
from decimal import Decimal, ROUND_HALF_UP
import sys

random.seed(560)

START = date(2023, 11, 1)       # first sales day
END = date(2026, 11, 17)        # last sales day (day before "today")
OPENING_PO = date(2023, 10, 20) # opening-stock purchase orders
PRICE_CHANGES = [date(2025, 1, 1), date(2026, 5, 1)]

suppliers = [
    "Northstar Game Distribution",
    "Prairie Hobby Wholesale",
    "Crossroads Gaming Supply",
    "Ironforge Distributors",
    "Blue Mesa Trade Co.",
]

product_types = [
    "Rulebook", "Adventure", "Dice", "Miniatures",
    "Hobby Supplies", "Card Deck", "Accessories", "Starter Set",
]

brands = [
    "Wizards of the Coast", "Paizo", "Games Workshop", "Chessex", "WizKids",
    "Reaper Miniatures", "The Army Painter", "Ultra PRO", "Chaosium",
    "Free League Publishing", "Kobold Press", "Gale Force Nine",
]

# brand -> supplier (1-based)
brand_supplier = {
    "Wizards of the Coast": 1, "Paizo": 1, "Kobold Press": 1,
    "Games Workshop": 4, "The Army Painter": 4, "Reaper Miniatures": 4,
    "Chessex": 2, "Ultra PRO": 2, "Gale Force Nine": 2,
    "WizKids": 3, "Chaosium": 3, "Free League Publishing": 5,
}

# (name, type, brand, sell price, popularity weight)
products = [
    ("Player's Handbook (2024)", "Rulebook", "Wizards of the Coast", 49.99, 9),
    ("Dungeon Master's Guide (2024)", "Rulebook", "Wizards of the Coast", 49.99, 6),
    ("Monster Manual (2025)", "Rulebook", "Wizards of the Coast", 49.99, 6),
    ("D&D Starter Set: Heroes of the Borderlands", "Starter Set", "Wizards of the Coast", 49.99, 5),
    ("Vecna: Eve of Ruin", "Adventure", "Wizards of the Coast", 59.99, 3),
    ("Quests from the Infinite Staircase", "Adventure", "Wizards of the Coast", 49.99, 3),
    ("Dungeon Master's Screen Dungeon Kit", "Accessories", "Wizards of the Coast", 29.99, 3),
    ("Pathfinder Player Core", "Rulebook", "Paizo", 59.99, 5),
    ("Pathfinder GM Core", "Rulebook", "Paizo", 59.99, 3),
    ("Pathfinder Monster Core", "Rulebook", "Paizo", 59.99, 3),
    ("Pathfinder Beginner Box", "Starter Set", "Paizo", 39.99, 3),
    ("Pathfinder Flip-Mat: Basic Terrain", "Accessories", "Paizo", 14.99, 3),
    ("Tome of Beasts 1 (2023 Edition)", "Rulebook", "Kobold Press", 59.99, 2),
    ("Tales of the Valiant Player's Guide", "Rulebook", "Kobold Press", 49.99, 2),
    ("Call of Cthulhu Keeper Rulebook", "Rulebook", "Chaosium", 59.95, 2),
    ("Call of Cthulhu Starter Set", "Starter Set", "Chaosium", 29.99, 2),
    ("Dragonbane Core Set", "Starter Set", "Free League Publishing", 49.99, 2),
    ("The One Ring Core Rules", "Rulebook", "Free League Publishing", 59.99, 2),
    ("ALIEN RPG Evolved Edition Core Rulebook", "Rulebook", "Free League Publishing", 54.99, 1),
    ("Chessex Gemini 7-Die Set: Blue-Gold", "Dice", "Chessex", 12.99, 8),
    ("Chessex Speckled 7-Die Set: Space", "Dice", "Chessex", 12.99, 8),
    ("Chessex Opaque 7-Die Set: Black/White", "Dice", "Chessex", 10.99, 7),
    ("Chessex Pound-O-Dice", "Dice", "Chessex", 29.99, 3),
    ("Chessex Battlemat 1-inch Squares", "Accessories", "Chessex", 31.99, 2),
    ("Nolzur's Marvelous Miniatures: Human Fighter", "Miniatures", "WizKids", 5.99, 6),
    ("Nolzur's Marvelous Miniatures: Goblins", "Miniatures", "WizKids", 5.99, 6),
    ("Nolzur's Marvelous Miniatures: Owlbear", "Miniatures", "WizKids", 9.99, 4),
    ("D&D Icons of the Realms Booster", "Miniatures", "WizKids", 15.99, 4),
    ("Reaper Bones Black: Skeleton Warriors (3)", "Miniatures", "Reaper Miniatures", 9.99, 3),
    ("Reaper Bones: Red Dragon", "Miniatures", "Reaper Miniatures", 39.99, 2),
    ("Citadel Base Paint: Abaddon Black", "Hobby Supplies", "Games Workshop", 5.25, 5),
    ("Citadel Layer Paint: White Scar", "Hobby Supplies", "Games Workshop", 5.25, 4),
    ("Citadel Shade Paint: Nuln Oil", "Hobby Supplies", "Games Workshop", 8.25, 5),
    ("Citadel Medium Base Brush", "Hobby Supplies", "Games Workshop", 8.50, 3),
    ("Army Painter Speedpaint Starter Set", "Hobby Supplies", "The Army Painter", 39.99, 3),
    ("Army Painter Matt Black Primer Spray", "Hobby Supplies", "The Army Painter", 17.99, 4),
    ("Army Painter Wargamer Character Brush", "Hobby Supplies", "The Army Painter", 6.99, 3),
    ("Ultra PRO Dice Tray: Black Felt", "Accessories", "Ultra PRO", 14.99, 3),
    ("Ultra PRO Eclipse Sleeves (100 ct)", "Accessories", "Ultra PRO", 10.99, 4),
    ("Ultra PRO Hero Miniature Case", "Accessories", "Ultra PRO", 24.99, 2),
    ("Spellbook Cards: Arcane Deck", "Card Deck", "Gale Force Nine", 29.99, 4),
    ("Spellbook Cards: Divine Deck", "Card Deck", "Gale Force Nine", 19.99, 3),
    ("Spellbook Cards: Martial Powers & Races", "Card Deck", "Gale Force Nine", 19.99, 2),
    ("D&D Condition Cards", "Card Deck", "Gale Force Nine", 12.99, 3),
    ("Pathfinder Critical Hit Deck", "Card Deck", "Paizo", 14.99, 2),
    # Older editions, replaced during the simulated period
    ("Player's Handbook (2014)", "Rulebook", "Wizards of the Coast", 49.95, 8),
    ("Dungeon Master's Guide (2014)", "Rulebook", "Wizards of the Coast", 49.95, 5),
    ("Monster Manual (2014)", "Rulebook", "Wizards of the Coast", 49.95, 5),
    ("D&D Essentials Kit", "Starter Set", "Wizards of the Coast", 24.99, 4),
    ("Pathfinder Core Rulebook (2nd Edition)", "Rulebook", "Paizo", 59.99, 4),
    ("Pathfinder Bestiary (2nd Edition)", "Rulebook", "Paizo", 49.99, 2),
]

# Products launched mid-year (not sold before their launch date)
launch = {
    "Pathfinder Player Core": date(2023, 11, 15),
    "Pathfinder GM Core": date(2023, 11, 15),
    "Vecna: Eve of Ruin": date(2024, 3, 19),
    "Pathfinder Monster Core": date(2024, 3, 20),
    "Tales of the Valiant Player's Guide": date(2024, 5, 15),
    "Quests from the Infinite Staircase": date(2024, 7, 16),
    "Player's Handbook (2024)": date(2024, 9, 17),
    "Dungeon Master's Guide (2024)": date(2024, 11, 12),
    "Monster Manual (2025)": date(2025, 2, 18),
    "D&D Starter Set: Heroes of the Borderlands": date(2025, 9, 9),
    "ALIEN RPG Evolved Edition Core Rulebook": date(2026, 4, 1),
}

# Products no longer reordered from this date (replaced by a new edition)
discontinued = {
    "Pathfinder Core Rulebook (2nd Edition)": date(2023, 11, 15),
    "Pathfinder Bestiary (2nd Edition)": date(2024, 3, 20),
    "Player's Handbook (2014)": date(2024, 9, 17),
    "Dungeon Master's Guide (2014)": date(2024, 11, 12),
    "Monster Manual (2014)": date(2025, 2, 18),
    "D&D Essentials Kit": date(2025, 9, 9),
}

# Seasonality multiplier by month (holiday spike, summer dip)
season = {1: 0.85, 2: 0.8, 3: 0.9, 4: 0.9, 5: 0.95, 6: 0.85, 7: 0.85,
          8: 0.95, 9: 1.0, 10: 1.05, 11: 1.3, 12: 1.6}

first_names = ["Avery", "Jordan", "Riley", "Morgan", "Casey", "Taylor", "Quinn",
               "Rowan", "Elena", "Marcus", "Priya", "Devon", "Hannah", "Isaac",
               "Mei", "Caleb", "Sofia", "Owen", "Nadia", "Ethan", "Lucia", "Grant",
               "Tessa", "Wesley", "Imani", "Dylan", "Harper", "Felix", "Jade", "Theo"]
last_names = ["Hollis", "Brennan", "Vasquez", "Okafor", "Lindqvist", "Reyes",
              "Nakamura", "Whitfield", "Castellanos", "Duval", "Kowalski", "Ashby",
              "Mendoza", "Pryce", "Abernathy", "Sato", "Fairchild", "Ibarra",
              "Thornton", "Delacroix", "Haddad", "Marlowe", "Quist", "Rourke"]

def money(x):
    return Decimal(str(x)).quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)

def round_retail(x):
    """Round a price to the nearest .99 / .49 style retail price."""
    d = money(x)
    whole = int(d)
    return money(whole + 0.99) if d - whole > Decimal("0.5") else money(whole + 0.49)

# ---------------- Products with price history ----------------
prod = []
for i, (name, ptype, brand, sell, pop) in enumerate(products, start=1):
    sell = money(sell)
    ratio = Decimal(str(random.choice([0.52, 0.55, 0.58, 0.60])))
    bulk = money(sell * ratio)
    p = dict(id=i, name=name, type=product_types.index(ptype) + 1,
             brand=brands.index(brand) + 1,
             supplier=brand_supplier[brand], pop=pop,
             base=sell, hist=[(date.min, sell, bulk)],
             launch=launch.get(name, START), disc=discontinued.get(name), stock=0)
    prod.append(p)

# On each price-change date, ~30% of products get a price increase
for change in PRICE_CHANGES:
    for p in random.sample(prod, k=round(len(prod) * 0.3)):
        _, sell, bulk = p["hist"][-1]
        pct = Decimal(str(random.choice([1.08, 1.10, 1.12])))
        p["hist"].append((change, round_retail(sell * pct), money(bulk * pct)))

def price(p, d):
    """(sell, bulk) price in effect on date d."""
    cur = p["hist"][0]
    for h in p["hist"]:
        if h[0] <= d:
            cur = h
    return cur[1], cur[2]

def current(p):
    return p["hist"][-1][1], p["hist"][-1][2]

# ---------------- Customers ----------------
names = set()
while len(names) < 120:
    names.add(f"{random.choice(first_names)} {random.choice(last_names)}")
names = sorted(names, key=lambda _: random.random())
customers = []
for i, n in enumerate(names, start=1):
    if i <= 50:
        opened = date(2020, 1, 1) + timedelta(days=random.randint(0, (date(2023, 10, 31) - date(2020, 1, 1)).days))
    else:  # newer accounts opened during the simulated year
        opened = START + timedelta(days=random.randint(0, (date(2026, 10, 15) - START).days))
    # Pareto-ish weights: a few regulars buy a lot
    weight = min(random.paretovariate(1.4), 6.0)
    customers.append(dict(id=i, name=n, opened=opened, w=weight))

# ---------------- Simulation ----------------
pos, po_lines = [], []          # (id, supplier, date), (id, po, product, qty, cost)
sos, so_lines = [], []          # (id, customer, date), (id, so, product, qty, price)

def target_stock(p):
    return 6 + p["pop"] * 3

def place_po(supplier_id, d, items):
    if not items:
        return
    po_id = len(pos) + 1
    pos.append((po_id, supplier_id, d))
    for p, qty in items:
        _, cost = price(p, d)
        po_lines.append((len(po_lines) + 1, po_id, p["id"], qty, cost))
        p["stock"] += qty

# Opening stock order (products available at START)
for s in range(1, len(suppliers) + 1):
    items = [(p, target_stock(p)) for p in prod if p["supplier"] == s and p["launch"] <= START]
    place_po(s, OPENING_PO, items)

d = START
while d <= END:
    # Launch-day stock for new products
    for p in prod:
        if p["launch"] == d:
            place_po(p["supplier"], d - timedelta(days=2), [(p, target_stock(p) * 2)])

    # Restock: each supplier is ordered from on Mondays if anything is low.
    # Holiday season gets bigger orders.
    if d.weekday() == 0:
        for s in range(1, len(suppliers) + 1):
            mult = 1.5 if d.month in (11, 12) else 1.0
            items = []
            for p in prod:
                if p["supplier"] != s or p["launch"] > d or (p["disc"] and d >= p["disc"]):
                    continue
                tgt = int(target_stock(p) * mult)
                if p["stock"] < tgt * 0.85:
                    items.append((p, tgt - p["stock"], p["stock"] < tgt * 0.5))
            # Only place an order when something is actually running low,
            # then top up everything from that supplier that is below 85%.
            if any(low for _, _, low in items):
                place_po(s, d, [(p, qty) for p, qty, _ in items])

    # Sales for the day (weekends busier)
    base = 3.8 if d.weekday() >= 5 else 2.4
    growth = 0.8 + 0.2 * (d - START).days / (END - START).days   # store grows over time
    n_orders = max(0, int(random.gauss(base * season[d.month] * growth, 1.1) + 0.5))
    eligible_c = [c for c in customers if c["opened"] <= d]
    for _ in range(n_orders):
        c = random.choices(eligible_c, weights=[c["w"] for c in eligible_c])[0]
        avail = [p for p in prod if p["launch"] <= d and p["stock"] > 0]
        if not avail:
            continue
        n_lines = random.choices([1, 2, 3, 4], weights=[45, 30, 17, 8])[0]
        chosen = set()
        lines = []
        for _ in range(n_lines):
            w = [p["pop"] * (2.5 if (p["launch"] != START and (d - p["launch"]).days < 45) else 1) for p in avail]
            p = random.choices(avail, weights=w)[0]
            if p["id"] in chosen:
                continue
            chosen.add(p["id"])
            cheap = p["base"] < 16
            qty = random.choices([1, 2, 3], weights=[70, 22, 8] if cheap else [92, 7, 1])[0]
            qty = min(qty, p["stock"])
            if qty <= 0:
                continue
            p["stock"] -= qty
            lines.append((p, qty))
        if not lines:
            continue
        so_id = len(sos) + 1
        sos.append((so_id, c["id"], d))
        for p, qty in lines:
            sell, _ = price(p, d)
            so_lines.append((len(so_lines) + 1, so_id, p["id"], qty, sell))
    d += timedelta(days=1)

# ---------------- Write SQL ----------------
def q(s):
    return "N'" + s.replace("'", "''") + "'"

out = []
w = out.append
w("/* =====================================================================")
w("   TTRPG Shop - Data.sql")
w("   Populates every table with sample data. Run after Tables.sql.")
w(f"   Simulated period: {START} to {END}.")
w("   - Stock is consistent: Products.QuantityOnHand = purchased - sold.")
w(f"   - Some prices increased on {', '.join(str(c) for c in PRICE_CHANGES)}, so UnitPrice /")
w("     UnitCost on older order lines differ from the current Products prices.")
w("   - Older editions (e.g. Player's Handbook (2014)) stop being restocked")
w("     when their replacement launches and sell through remaining stock.")
w("   ===================================================================== */")
w("")
w("SET NOCOUNT ON;")
w("GO")
w("")

def insert_block(table, cols, rows, identity=True, chunk=500):
    w(f"/* ---- {table} ({len(rows)} rows) ---- */")
    if identity:
        w(f"SET IDENTITY_INSERT CIS560.{table} ON;")
    for i in range(0, len(rows), chunk):
        part = rows[i:i + chunk]
        w(f"INSERT INTO CIS560.{table} ({', '.join(cols)})")
        w("VALUES")
        w(",\n".join("    (" + ", ".join(r) + ")" for r in part) + ";")
    if identity:
        w(f"SET IDENTITY_INSERT CIS560.{table} OFF;")
    w("GO")
    w("")

insert_block("Suppliers", ["SupplierID", "SupplierName"],
             [(str(i), q(n)) for i, n in enumerate(suppliers, 1)])
insert_block("ProductTypes", ["ProductTypeID", "ProductTypeName"],
             [(str(i), q(n)) for i, n in enumerate(product_types, 1)])
insert_block("Brands", ["BrandID", "BrandName"],
             [(str(i), q(n)) for i, n in enumerate(brands, 1)])
insert_block("Customers", ["CustomerID", "CustomerName", "AccountOpenedDate"],
             [(str(c["id"]), q(c["name"]), f"'{c['opened']}'") for c in customers])
insert_block("Products",
             ["ProductID", "SupplierID", "ProductTypeID", "BrandID", "ProductName",
              "QuantityOnHand", "BulkPrice", "SellPrice"],
             [(str(p["id"]), str(p["supplier"]), str(p["type"]), str(p["brand"]), q(p["name"]),
               str(p["stock"]), str(current(p)[1]), str(current(p)[0])) for p in prod])
insert_block("PurchaseOrder", ["PurchaseOrderID", "SupplierID", "OrderDate"],
             [(str(a), str(b), f"'{c}'") for a, b, c in pos])
insert_block("PurchaseOrderLines",
             ["PurchaseOrderLinesID", "PurchaseOrderID", "ProductID", "Quantity", "UnitCost"],
             [tuple(str(x) for x in r) for r in po_lines])
insert_block("CustomerSalesOrder", ["CustomerSalesOrderID", "CustomerID", "OrderDate"],
             [(str(a), str(b), f"'{c}'") for a, b, c in sos])
insert_block("CustomerSalesOrderLines",
             ["CustomerSalesOrderLinesID", "CustomerSalesOrderID", "ProductID", "Quantity", "UnitPrice"],
             [tuple(str(x) for x in r) for r in so_lines])

with open(sys.argv[1], "w", encoding="utf-8") as f:
    f.write("\n".join(out) + "\n")

print(f"suppliers={len(suppliers)} types={len(product_types)} brands={len(brands)} "
      f"customers={len(customers)} products={len(prod)} POs={len(pos)} PO lines={len(po_lines)} "
      f"SOs={len(sos)} SO lines={len(so_lines)}")
