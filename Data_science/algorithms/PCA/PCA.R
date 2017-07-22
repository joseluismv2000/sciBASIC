d <- read.csv("./data.csv");

head(d);

# ԭʼ���ݱ�׼��
sd <- scale(d);
head(sd);

d <- sd;
pca <- princomp(d, cor=T);
screeplot(pca, type="line", main="��ʯͼ", lwd=2);

# ����ؾ���
dcor <- cor(d);
dcor;

# ����ؾ������������ ����ֵ
deig <- eigen(dcor);
deig;

# �������ֵ
deig$values;
sumeigv <- sum(deig$values)
sumeigv;

# ��ǰ2�����ɷֵ��ۻ��������
sum(deig$value[1:2])/4
sum(deig$value[1:1])/4

# ���ǰ�������ɷֵ��غ�ϵ��������������
pca$loadings[,1:2]

# �������ɷ�C1��C2��ϵ��b1 ��b2��
deig$values[1]/4;
deig$values[2]/4;

# ���ǰ2 �����ɷֵĵ÷�
s <- pca$scores[,1:2]