#!/usr/bin/env python3
"""
eval-viewer: Skill テストケースのビジュアルレビュー画面を生成

使用方法:
  python generate_review.py ./evals/ --output review.html

このスクリプトは evals.json と実行結果から HTML レビュー画面を生成します。
Outputs タブと Benchmark タブで、スキル有り/無しの出力を比較表示できます。
"""

import json
import sys
from pathlib import Path
from datetime import datetime
from typing import Optional

def load_eval_results(evals_dir: Path) -> dict:
    """evals/ ディレクトリから evals.json と results.json を読み込む"""
    evals_file = evals_dir / "evals.json"
    results_file = evals_dir / "results.json"
    
    evals = {}
    results = {}
    
    if evals_file.exists():
        with open(evals_file, 'r', encoding='utf-8') as f:
            evals = json.load(f)
    
    if results_file.exists():
        with open(results_file, 'r', encoding='utf-8') as f:
            results = json.load(f)
    
    return {"evals": evals, "results": results}

def generate_html(data: dict, output_file: Path) -> None:
    """HTML レビュー画面を生成"""
    
    evals = data.get("evals", {})
    results = data.get("results", {})
    
    # Outputs タブ用の HTML を生成
    outputs_html = ""
    for test_case in evals.get("evals", []):
        test_id = test_case.get("id", 0)
        prompt = test_case.get("prompt", "")
        result = results.get(f"test_{test_id}", {})
        
        with_skill = result.get("with_skill", {"output": "実行未完了", "tokens": 0, "time": 0})
        without_skill = result.get("without_skill", {"output": "実行未完了", "tokens": 0, "time": 0})
        
        outputs_html += f"""
        <div class="test-case">
            <h3>テストケース {test_id}: {prompt}</h3>
            <div class="comparison">
                <div class="skill-with">
                    <h4>スキル有り</h4>
                    <pre>{with_skill.get('output', 'N/A')}</pre>
                    <p>トークン数: {with_skill.get('tokens', 'N/A')}, 実行時間: {with_skill.get('time', 'N/A')}s</p>
                </div>
                <div class="skill-without">
                    <h4>スキル無し（ベースライン）</h4>
                    <pre>{without_skill.get('output', 'N/A')}</pre>
                    <p>トークン数: {without_skill.get('tokens', 'N/A')}, 実行時間: {without_skill.get('time', 'N/A')}s</p>
                </div>
            </div>
            <textarea placeholder="このテストケースについてのフィードバックを入力..." class="feedback" data-test-id="{test_id}"></textarea>
        </div>
        """
    
    # Benchmark タブ用の統計情報
    total_tests = len(evals.get("evals", []))
    passed_tests = sum(1 for r in results.values() if r.get("status") == "PASS")
    
    benchmark_html = f"""
    <div class="benchmark">
        <h3>テスト統計</h3>
        <ul>
            <li>総テスト数: {total_tests}</li>
            <li>成功数: {passed_tests}</li>
            <li>成功率: {passed_tests * 100 // total_tests if total_tests > 0 else 0}%</li>
        </ul>
    </div>
    """
    
    # 完全な HTML ドキュメントを生成
    html_content = f"""<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Skill テストレビュー - {evals.get('skill_name', 'Unknown')}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            margin: 0;
            padding: 20px;
            background: #f5f5f5;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            padding: 20px;
        }}
        h1 {{
            color: #333;
            border-bottom: 2px solid #0366d6;
            padding-bottom: 10px;
        }}
        .tabs {{
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
            border-bottom: 1px solid #ddd;
        }}
        .tab-button {{
            padding: 10px 20px;
            background: none;
            border: none;
            cursor: pointer;
            font-size: 1rem;
            color: #666;
            border-bottom: 3px solid transparent;
        }}
        .tab-button.active {{
            color: #0366d6;
            border-bottom-color: #0366d6;
        }}
        .tab-content {{
            display: none;
        }}
        .tab-content.active {{
            display: block;
        }}
        .test-case {{
            margin-bottom: 30px;
            padding: 15px;
            background: #fafafa;
            border-radius: 6px;
            border-left: 4px solid #0366d6;
        }}
        .comparison {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin: 15px 0;
        }}
        .skill-with, .skill-without {{
            background: white;
            padding: 15px;
            border-radius: 6px;
        }}
        .skill-with h4 {{
            color: #28a745;
        }}
        .skill-without h4 {{
            color: #6f42c1;
        }}
        pre {{
            background: #f6f8fa;
            padding: 10px;
            border-radius: 4px;
            overflow-x: auto;
            max-height: 200px;
            overflow-y: auto;
        }}
        textarea.feedback {{
            width: 100%;
            height: 80px;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-family: monospace;
        }}
        .benchmark {{
            padding: 20px;
            background: #f6f8fa;
            border-radius: 6px;
        }}
        .benchmark ul {{
            list-style: none;
            padding: 0;
        }}
        .benchmark li {{
            padding: 8px 0;
            border-bottom: 1px solid #ddd;
        }}
        .submit-button {{
            padding: 10px 20px;
            background: #28a745;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 1rem;
        }}
        .submit-button:hover {{
            background: #218838;
        }}
        .timestamp {{
            color: #666;
            font-size: 0.9rem;
        }}
    </style>
</head>
<body>
    <div class="container">
        <h1>Skill テストレビュー</h1>
        <p>スキル: <strong>{evals.get('skill_name', 'Unknown')}</strong></p>
        <p class="timestamp">生成日時: {datetime.now().isoformat()}</p>
        
        <div class="tabs">
            <button class="tab-button active" onclick="showTab('outputs')">Outputs</button>
            <button class="tab-button" onclick="showTab('benchmark')">Benchmark</button>
        </div>
        
        <div id="outputs" class="tab-content active">
            {outputs_html}
            <button class="submit-button" onclick="submitFeedback()">Submit All Reviews</button>
        </div>
        
        <div id="benchmark" class="tab-content">
            {benchmark_html}
        </div>
    </div>
    
    <script>
        function showTab(tabName) {{
            // すべてのタブを非表示
            document.querySelectorAll('.tab-content').forEach(el => {{
                el.classList.remove('active');
            }});
            document.querySelectorAll('.tab-button').forEach(el => {{
                el.classList.remove('active');
            }});
            
            // 指定されたタブを表示
            document.getElementById(tabName).classList.add('active');
            event.target.classList.add('active');
        }}
        
        function submitFeedback() {{
            const feedback = {{}};
            document.querySelectorAll('textarea.feedback').forEach(el => {{
                const testId = el.getAttribute('data-test-id');
                feedback[testId] = el.value;
            }});
            
            // feedback.json として保存（実装例）
            console.log('Feedback:', feedback);
            alert('フィードバックを記録しました。Claude に共有してください。\\n\\n' + JSON.stringify(feedback, null, 2));
        }}
    </script>
</body>
</html>
"""
    
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(html_content)
    
    print(f"✓ HTML レビュー画面を生成しました: {output_file}")

def main():
    """メイン処理"""
    if len(sys.argv) < 2:
        print("使用方法: python generate_review.py <evals_dir> --output <output_file>")
        sys.exit(1)
    
    evals_dir = Path(sys.argv[1])
    output_file = Path("review.html")
    
    # コマンドライン引数を解析
    if "--output" in sys.argv:
        idx = sys.argv.index("--output")
        if idx + 1 < len(sys.argv):
            output_file = Path(sys.argv[idx + 1])
    
    if not evals_dir.exists():
        print(f"エラー: ディレクトリが見つかりません: {evals_dir}")
        sys.exit(1)
    
    # データを読み込んで HTML を生成
    data = load_eval_results(evals_dir)
    generate_html(data, output_file)

if __name__ == "__main__":
    main()
