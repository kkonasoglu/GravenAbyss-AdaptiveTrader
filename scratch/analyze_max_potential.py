import os
import glob
import pandas as pd

veriler_dir = r"bin\Debug\Veriler"
csv_files = glob.glob(os.path.join(veriler_dir, "*.csv"))

results = []

for f in csv_files:
    basename = os.path.basename(f).upper()
    try:
        df = pd.read_csv(f, sep=None, engine='python')
        date_col = [c for c in df.columns if 'date' in c.lower() or 'tarih' in c.lower()]
        price_col = [c for c in df.columns if 'price' in c.lower() or 'kapanis' in c.lower() or 'close' in c.lower() or 'son' in c.lower() or 'şimdi' in c.lower() or 'simdi' in c.lower()]
        
        if date_col and price_col:
            df['Date'] = pd.to_datetime(df[date_col[0]], errors='coerce')
            df['Price'] = df[price_col[0]].astype(str).str.replace('.', '').str.replace(',', '.').str.replace('TL', '').str.strip()
            df['Price'] = pd.to_numeric(df['Price'], errors='coerce')
            df = df.dropna(subset=['Date', 'Price']).sort_values('Date')
            
            df_2025 = df[df['Date'].dt.year == 2025]
            df_2026 = df[df['Date'].dt.year == 2026]
            
            p_2025_start = df_2025['Price'].iloc[0] if len(df_2025) > 0 else None
            p_2025_end = df_2025['Price'].iloc[-1] if len(df_2025) > 0 else None
            p_2026_end = df_2026['Price'].iloc[-1] if len(df_2026) > 0 else None
            
            ret_2025 = ((p_2025_end - p_2025_start) / p_2025_start * 100) if (p_2025_start and p_2025_end) else 0
            ret_2026 = ((p_2026_end - p_2025_end) / p_2025_end * 100) if (p_2026_end and p_2025_end) else 0
            ret_total = ((p_2026_end - p_2025_start) / p_2025_start * 100) if (p_2025_start and p_2026_end) else 0
            
            results.append({
                'Sembol': basename.split(' ')[0],
                '2025_Start': p_2025_start,
                '2025_End': p_2025_end,
                '2026_End': p_2026_end,
                '2025_Getiri_%': round(ret_2025, 2),
                '2026_Getiri_%': round(ret_2026, 2),
                '2_Yillik_Getiri_%': round(ret_total, 2)
            })
    except Exception as e:
        print(f"Hata {f}: {e}")

if results:
    res_df = pd.DataFrame(results).sort_values('2_Yillik_Getiri_%', ascending=False)
    print("\n--- GERÇEK HİSSE PERFORMANSLARI ---")
    print(res_df.to_string(index=False))
