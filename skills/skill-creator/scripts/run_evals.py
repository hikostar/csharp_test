#!/usr/bin/env python3
"""
run_evals.py: Skill テストケースの実行とベースライン比較

使用方法:
  python -m scripts.run_evals evals/evals.json
  python -m scripts.run_evals evals/evals.json --verbose

このスクリプトは:
1. evals.json からテストケースを読み込む
2. スキル有りで実行
3. スキル無し（ベースライン）で実行
4. 結果を比較して JSON に保存
5. パス/失敗を表示
"""

import json
import sys
from pathlib import Path
from datetime import datetime
from typing import Dict, Any, Optional
import time

# 注: 実際の運用では anthropic SDK を使用
# from anthropic import Anthropic

def load_evals(evals_file: Path) -> Dict[str, Any]:
    """evals.json を読み込む"""
    if not evals_file.exists():
        print(f"エラー: {evals_file} が見つかりません")
        sys.exit(1)
    
    with open(evals_file, 'r', encoding='utf-8') as f:
        return json.load(f)

def run_test_with_skill(prompt: str) -> Dict[str, Any]:
    """スキルを使用してテストを実行"""
    # 実装例: Claude API を呼び出してスキルを使用
    print(f"  [スキル有り] 実行中: {prompt[:50]}...")
    
    # シミュレーション用
    time.sleep(0.5)
    return {
        "output": "スキル有りの出力結果サンプル",
        "tokens": 2340,
        "time": 2.3,
        "status": "PASS"
    }

def run_test_without_skill(prompt: str) -> Dict[str, Any]:
    """スキルなしでテストを実行（ベースライン）"""
    # 実装例: Claude API を呼び出してスキルなしで実行
    print(f"  [ベースライン] 実行中: {prompt[:50]}...")
    
    # シミュレーション用
    time.sleep(0.6)
    return {
        "output": "スキルなし（ベースライン）の出力結果",
        "tokens": 3200,
        "time": 2.5,
        "status": "PASS"
    }

def check_assertions(output: str, assertions: list) -> Dict[str, Any]:
    """Assertion を確認"""
    results = {}
    
    for assertion in assertions:
        assertion_text = assertion.get("text", "")
        assertion_type = assertion.get("type", "contains")
        
        if assertion_type == "contains":
            value = assertion.get("value", "")
            passed = value.lower() in output.lower()
        elif assertion_type == "regex":
            import re
            pattern = assertion.get("pattern", "")
            passed = bool(re.search(pattern, output))
        else:  # qualitative
            # 定性的評価は人間が行う
            passed = None
        
        results[assertion_text] = {
            "type": assertion_type,
            "status": "PASS" if passed else ("N/A" if passed is None else "FAIL")
        }
    
    return results

def run_all_tests(evals: Dict[str, Any], verbose: bool = False) -> Dict[str, Any]:
    """すべてのテストを実行"""
    
    skill_name = evals.get("skill_name", "unknown")
    test_cases = evals.get("evals", [])
    
    print(f"\n{'='*60}")
    print(f"Skill: {skill_name}")
    print(f"テストケース数: {len(test_cases)}")
    print(f"{'='*60}\n")
    
    results = {
        "skill_name": skill_name,
        "timestamp": datetime.now().isoformat(),
        "total_tests": len(test_cases),
        "passed": 0,
        "failed": 0,
        "tests": {}
    }
    
    for test_case in test_cases:
        test_id = test_case.get("id", 0)
        prompt = test_case.get("prompt", "")
        assertions = test_case.get("assertions", [])
        is_baseline = test_case.get("is_baseline", False)
        
        print(f"[テスト {test_id}] {prompt[:60]}")
        
        if is_baseline:
            # ベースラインテスト（スキルなしのみ実行）
            without_skill = run_test_without_skill(prompt)
            test_result = {
                "prompt": prompt,
                "with_skill": None,
                "without_skill": without_skill,
                "status": without_skill.get("status", "UNKNOWN")
            }
        else:
            # 通常テスト（スキル有り/無しを並列実行）
            with_skill = run_test_with_skill(prompt)
            without_skill = run_test_without_skill(prompt)
            
            # Assertion をチェック
            with_skill_assertions = check_assertions(with_skill["output"], assertions)
            
            # スキル有りが有効かどうかを判定
            token_reduction = (without_skill["tokens"] - with_skill["tokens"]) / without_skill["tokens"] * 100
            
            status = "PASS" if all(r["status"] == "PASS" for r in with_skill_assertions.values()) else "FAIL"
            
            test_result = {
                "prompt": prompt,
                "with_skill": with_skill,
                "without_skill": without_skill,
                "assertions": with_skill_assertions,
                "token_reduction": f"{token_reduction:.1f}%",
                "status": status
            }
            
            if verbose:
                print(f"  スキル有り出力: {with_skill['output'][:100]}...")
                print(f"  ベースライン:  {without_skill['output'][:100]}...")
                print(f"  トークン削減: {token_reduction:.1f}%")
                for assertion_text, result in with_skill_assertions.items():
                    print(f"    {result['status']}: {assertion_text}")
        
        # 結果を記録
        results["tests"][f"test_{test_id}"] = test_result
        
        if test_result["status"] == "PASS":
            results["passed"] += 1
            print(f"  ✓ PASS\n")
        else:
            results["failed"] += 1
            print(f"  ✗ FAIL\n")
    
    return results

def save_results(results: Dict[str, Any], output_file: Path) -> None:
    """テスト結果を JSON に保存"""
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(results, f, ensure_ascii=False, indent=2)
    print(f"✓ テスト結果を保存しました: {output_file}")

def print_summary(results: Dict[str, Any]) -> None:
    """テスト結果サマリーを表示"""
    print(f"\n{'='*60}")
    print(f"テスト結果サマリー")
    print(f"{'='*60}")
    print(f"総テスト数:    {results['total_tests']}")
    print(f"成功:         {results['passed']}")
    print(f"失敗:         {results['failed']}")
    print(f"成功率:       {results['passed'] * 100 // results['total_tests'] if results['total_tests'] > 0 else 0}%")
    print(f"{'='*60}\n")

def main():
    """メイン処理"""
    if len(sys.argv) < 2:
        print("使用方法: python -m scripts.run_evals <evals_file> [--verbose]")
        sys.exit(1)
    
    evals_file = Path(sys.argv[1])
    verbose = "--verbose" in sys.argv
    
    # evals.json を読み込み
    evals = load_evals(evals_file)
    
    # すべてのテストを実行
    results = run_all_tests(evals, verbose)
    
    # 結果を保存
    results_file = evals_file.parent / "results.json"
    save_results(results, results_file)
    
    # サマリーを表示
    print_summary(results)

if __name__ == "__main__":
    main()
