import os
import pandas as pd
import matplotlib.pyplot as plt

log_dir = "../logs/multicast-healthy"

records = []
for filename in os.listdir(log_dir):
    if not filename.endswith(".log"):
        continue
    node_id = filename.replace(".log", "")
    with open(os.path.join(log_dir, filename), "r") as f:
        for line in f:
            parts = line.strip().split(";")
            if len(parts) != 5: continue
            timestamp, event, receiver, sender, payload = parts
            if event != "RECEIVED": continue
            records.append({
                "timestamp": pd.to_datetime(timestamp),
                "receiver": receiver,
                "sender": sender,
                "message_id": payload
            })

df = pd.DataFrame(records)
df = df.sort_values("timestamp")

first_id = df["message_id"].value_counts().idxmax()
df_first = df[df["message_id"] == first_id]

df_first["t_seconds"] = (df_first["timestamp"] - df_first["timestamp"].min()).dt.total_seconds()
df_first = df_first.sort_values("t_seconds")
df_first["cumulative"] = range(1, len(df_first) + 1)

plt.figure(figsize=(10, 6))
plt.plot(df_first["t_seconds"], df_first["cumulative"], marker="o")
plt.xlabel("Seconds since first delivery")
plt.ylabel("Number of nodes that received message")
plt.title(f"Propagation of message '{first_id}'")
plt.grid(True)
plt.tight_layout()
plt.savefig("propagation_curve.png")
plt.show()