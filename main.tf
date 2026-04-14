module "eks" {
  source          = "terraform-aws-modules/eks/aws"
  version         = "21.18.0"

  cluster_name    = var.cluster_name
  cluster_version = var.eks_version
  vpc_id          = var.vpc_id
  subnet_ids      = var.subnet_ids

  eks_managed_node_groups = {
    default = {
      desired_size = 2
      max_size     = 2
      min_size     = 1
      instance_types = ["t3.medium"]
    }
  }

  tags = {
    Environment = "dev"
    Project     = "minimal-eks"
    ManagedBy   = "terraform"
  }
}