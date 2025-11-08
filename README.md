<h3 align="center"><a href="" style="color:#9C276A">
Paper Folding Puzzles: A Comprehensive Benchmark for Visual Reasoning and Spatial Intelligence
</a></h3>

<h5 align="center">

[![arXiv](https://img.shields.io/badge/Arxiv-2510.01304-AD1C18.svg?logo=arXiv)](https://arxiv.org/abs/2510.01304) 
[![Dataset](https://img.shields.io/badge/Dataset-PaperFolding-blue.svg)](https://github.com/hznuer/PFP_bench)
[![Project Page](https://img.shields.io/badge/ProjectPage-Website-green.svg?logo=github)](https://hznuer.github.io/PFP_bench/)

</h5>

## 🔍 Introduction

This repository contains the official implementation and benchmark dataset for our research on **Paper Folding Puzzles (PFP)** - a novel benchmark designed to evaluate and enhance visual reasoning and spatial intelligence in AI systems.

> Paper folding puzzles represent a fundamental challenge in visual reasoning, requiring models to understand geometric transformations, spatial relationships, and sequential reasoning. Our comprehensive benchmark provides a systematic framework for evaluating these capabilities across multiple complexity levels.

## 📋 Overview

**Paper Folding Puzzles** offer a unique lens into visual reasoning capabilities, testing:
- **Spatial Transformation Understanding**: How models comprehend geometric changes
- **Sequential Reasoning**: Following multi-step folding procedures  
- **Visual Pattern Recognition**: Identifying patterns in folded configurations
- **3D Spatial Visualization**: Mental reconstruction of 2D to 3D transformations

## 🚀 Key Features

- **Comprehensive Benchmark**: Multi-level puzzle complexity from basic to advanced
- **Rich Dataset**: Diverse paper folding scenarios with detailed annotations
- **Evaluation Framework**: Systematic assessment of visual reasoning capabilities
- **Scalable Generation**: Automated dataset creation for extensive testing

## 📊 Dataset and Code Availability

**Code and Dataset will be released upon paper acceptance.**

We are committed to making our research reproducible and accessible to the community. Once our paper is accepted for publication, we will release:

- **Dataset Generation Code**: Complete pipeline for creating paper folding puzzles
- **Comprehensive Benchmark Dataset**: Extensive collection of annotated puzzles
- **Model Evaluation Framework**: Standardized testing and evaluation tools
- **Baseline Models**: Reference implementations and pretrained models
- **Detailed Documentation**: Usage instructions and API documentation

## 🔧 Installation

```bash
# Clone the repository
git clone https://github.com/hznuer/PFP_bench.git
cd PFP_bench

# Create conda environment
conda create -n pfp python=3.10
conda activate pfp

# Install dependencies (available upon release)
pip install -r requirements.txt
```

## 🛫 Quick Start

### Dataset Structure

Upon release, the dataset will be organized as follows:

```
PFP_Dataset/
├── basic/
│   ├── single_fold/
│   ├── double_fold/
│   └── annotations.json
├── intermediate/
│   ├── complex_folds/
│   ├── pattern_recognition/
│   └── annotations.json
├── advanced/
│   ├── multi_step_reasoning/
│   ├── spatial_visualization/
│   └── annotations.json
└── metadata/
    ├── puzzle_types.json
    ├── difficulty_levels.json
    └── evaluation_metrics.json
```

### Evaluation Pipeline

```python
# Example usage (available upon release)
from pfp_benchmark import PaperFoldingBenchmark

# Initialize benchmark
benchmark = PaperFoldingBenchmark(dataset_path="PFP_Dataset/")

# Evaluate model
results = benchmark.evaluate(model=your_model)
print(f"Overall Accuracy: {results['accuracy']}")
print(f"Spatial Reasoning Score: {results['spatial_reasoning']}")
```

## 📈 Experimental Results

Our comprehensive evaluation demonstrates significant improvements in visual reasoning capabilities:

- **Baseline Performance**: Establishing fundamental benchmarks for paper folding tasks
- **Model Comparison**: Systematic evaluation across different architecture types
- **Ablation Studies**: Detailed analysis of key components and their contributions
- **Generalization Analysis**: Performance transfer to related visual reasoning tasks

*Detailed experimental results and analysis will be available upon paper acceptance.*

## 📚 Citation

If you find our work useful for your research, please cite:

```bibtex
@article{paperfolding2025benchmark,
  title={Paper Folding Puzzles: A Comprehensive Benchmark for Visual Reasoning and Spatial Intelligence},
  author={[Author Names]},
  journal={arXiv preprint arXiv:2510.01304},
  year={2025}
}
```

## 📞 Contact

For questions or inquiries, please:
- Open an issue in this repository
- Contact the authors through the paper correspondence
- Check our [project page](https://hznuer.github.io/PFP_bench/) for updates

---

**Stay tuned for the full release upon paper acceptance!** 🎉

*This repository will be updated with complete implementation, datasets, and documentation upon paper acceptance.*